namespace PayEx.Checkout.Episerver
{
	using EPiServer.Business.Commerce.Exception;
	using EPiServer.Commerce.Order;
	using EPiServer.Globalization;
	using EPiServer.Logging;
	using EPiServer.ServiceLocation;
	using EPiServer.Web;
	using Mediachase.Commerce;
	using Mediachase.Commerce.Markets;
	using Mediachase.Commerce.Orders.Dto;
	using Mediachase.Commerce.Orders.Managers;
	using PayEx.Checkout.Episerver.Common;
	using PayEx.Checkout.Episerver.Common.Helpers;
	using PayEx.Checkout.Episerver.Helpers;
	using PayEx.Checkout.Episerver.OrderManagement;
	using PayEx.Net.Api;
	using PayEx.Net.Api.Exceptions;
	using PayEx.Net.Api.Models;
	using System;
	using System.Globalization;
	using System.Linq;
	using System.Web;

	[ServiceConfiguration(typeof(IPayExCheckoutService))]
	public class PayExCheckoutService : IPayExCheckoutService
	{
		private readonly ICurrentMarket _currentMarket;
		private readonly ICheckoutConfigurationLoader _checkoutConfigurationLoader;
		private readonly IMarketService _marketService;
		private readonly IOrderGroupCalculator _orderGroupCalculator;
		private readonly IOrderRepository _orderRepository;
		private readonly PayExOrderServiceFactory _payExOrderServiceFactory;
		private readonly ILogger _logger = LogManager.GetLogger(typeof(PayExCheckoutService));
		private PaymentMethodDto _paymentMethodDto;
		private CheckoutConfiguration _checkoutConfiguration;

		public PaymentMethodDto PaymentMethodDto => _paymentMethodDto ?? (_paymentMethodDto = PaymentManager.GetPaymentMethodBySystemName(Constants.PayExCheckoutSystemKeyword, ContentLanguage.PreferredCulture.Name, returnInactive: true));

		public PayExCheckoutService(
			ICurrentMarket currentMarket,
			ICheckoutConfigurationLoader checkoutConfigurationLoader,
			IMarketService marketService,
			IOrderGroupCalculator orderGroupCalculator,
			IOrderRepository orderRepository,
			PayExOrderServiceFactory payExOrderServiceFactory)
		{
			_currentMarket = currentMarket;
			_checkoutConfigurationLoader = checkoutConfigurationLoader;
			_marketService = marketService;
			_orderGroupCalculator = orderGroupCalculator;
			_orderRepository = orderRepository;
			_payExOrderServiceFactory = payExOrderServiceFactory;
		}

		public CheckoutConfiguration GetCheckoutConfiguration(IMarket market)
		{
			return _checkoutConfiguration ?? (_checkoutConfiguration = GetConfiguration(market));
		}

		public PaymentOrderResponseObject CreateOrUpdateOrder(IOrderGroup orderGroup, string userAgent, string consumerProfileRef = null)
		{
			var orderId = orderGroup.Properties[Constants.PayExCheckoutOrderIdCartField]?.ToString();
			return string.IsNullOrWhiteSpace(orderId) ? CreateOrder(orderGroup, userAgent, consumerProfileRef) : UpdateOrder(orderId, orderGroup, userAgent);
		}

		public virtual InitiateConsumerSessionResponseObject InitiateConsumerSession(string email = null, string mobilePhone = null, string ssn = null)
		{
			var payExApi = GetPayExApi();
			var market = _marketService.GetMarket(_currentMarket.GetCurrentMarket().MarketId);

			var twoLetterIsoRegionName = new RegionInfo(market.DefaultLanguage.TextInfo.CultureName).TwoLetterISORegionName;

			var initiateConsumerSessionRequestObject = new InitiateConsumerSessionRequestObject
			{
				Email = email,
				Msisdn = mobilePhone,
				ConsumerCountryCode = twoLetterIsoRegionName
			};

			if (!string.IsNullOrWhiteSpace(ssn))
			{
				initiateConsumerSessionRequestObject.NationalIdentifier = new NationalIdentifier
				{
					CountryCode = twoLetterIsoRegionName,
					SocialSecurityNumber = ssn
				};
			}

			try
			{
				var initiateConsumerSessionResponseObject = payExApi.Consumers.InitiateConsumerSession(initiateConsumerSessionRequestObject);
				return initiateConsumerSessionResponseObject;
			}
			catch (PayExException ex)
			{
				_logger.Error($"{ex.ErrorCode} - {ex.Error.Title}... {ex.Error.Detail}::: {string.Join(", ", ex.Error.Problems.Select(x => $"{x.Name}: {x.Description}"))}");
				throw;
			}
			catch (Exception ex)
			{
				_logger.Error(ex.Message, ex);
				throw;
			}
		}


		public virtual ShippingDetails GetShippingDetails(string uri)
		{
			var payExApi = GetPayExApi();
			return payExApi.Consumers.GetShippingDetails(uri);
		}

		public virtual PaymentOrderResponseObject CreateOrder(IOrderGroup orderGroup, string userAgent, string consumerProfileRef = null)
		{
			var payExApi = GetPayExApi(orderGroup);
			var market = _marketService.GetMarket(orderGroup.MarketId);

			try
			{
				var paymentOrderRequestObject = GetCheckoutOrderData(orderGroup, market, PaymentMethodDto, consumerProfileRef);
				var paymentOrderResponseObject = payExApi.PaymentOrders.CreatePaymentOrder(paymentOrderRequestObject);

                orderGroup.Properties[Constants.PayExCheckoutOrderIdCartField] = paymentOrderResponseObject.PaymentOrder.Id;

				_orderRepository.Save(orderGroup);
				return paymentOrderResponseObject;
			}
			catch (PayExException ex)
			{
				_logger.Error($"{ex.ErrorCode} - {ex.Error.Title}... {ex.Error.Detail}::: {string.Join(", ", ex.Error.Problems.Select(x => $"{x.Name}: {x.Description}"))}");
				throw;
			}
			catch (Exception ex)
			{
				_logger.Error(ex.Message, ex);
				throw;
			}
		}



		public virtual PaymentOrderResponseObject UpdateOrder(string orderId, IOrderGroup orderGroup, string userAgent)
		{
			var market = _marketService.GetMarket(orderGroup.MarketId);
			var payExApi = GetPayExApi(orderGroup);

            var paymentOrderRequestObject = GetCheckoutOrderData(orderGroup, market, PaymentMethodDto);
			var paymentOrderResponseObject = payExApi.PaymentOrders.UpdatePaymentOrder(paymentOrderRequestObject, orderId);

            orderGroup.Properties[Constants.PayExCheckoutOrderIdCartField] = paymentOrderResponseObject.PaymentOrder.Id;
			_orderRepository.Save(orderGroup);

			return paymentOrderResponseObject;
		}

		public virtual PaymentOrderResponseObject GetOrder(string id, IMarket market, PaymentOrderExpand paymentOrderExpand = PaymentOrderExpand.None)
		{
			var checkoutConfiguration = GetCheckoutConfiguration(market);
			var payExApi = new PayExApi(checkoutConfiguration.ApiUrl, checkoutConfiguration.Token);

			try
			{
				var paymentOrderResponseObject = payExApi.PaymentOrders.GetPaymentOrder(id, paymentOrderExpand);
				return paymentOrderResponseObject;
			}
			catch (PayExException ex)
			{
				_logger.Error($"{ex.ErrorCode} - {ex.Error.Title}... {ex.Error.Detail}::: {string.Join(", ", ex.Error.Problems.Select(x => $"{x.Name}: {x.Description}"))}");
				throw;
			}
			catch (Exception ex)
			{
				_logger.Error(ex.Message, ex);
				throw;
			}
		}


		public void CancelOrder(IOrderGroup orderGroup)
		{
			var payExOrderService = _payExOrderServiceFactory.Create(GetConfiguration(orderGroup.MarketId));

			var orderId = orderGroup.Properties[Constants.PayExCheckoutOrderIdCartField]?.ToString();
			if (!string.IsNullOrWhiteSpace(orderId))
			{
				try
				{
					var cancelResponseObject = payExOrderService.CancelOrder(orderId);
					if (cancelResponseObject != null)
					{
                        orderGroup.Properties[Constants.PayExCheckoutOrderIdCartField] = null;
						_orderRepository.Save(orderGroup);
					}

				}
				catch (PayExException ex)
				{
					_logger.Error($"{ex.ErrorCode} - {ex.Error.Title}... {ex.Error.Detail}::: {string.Join(", ", ex.Error.Problems.Select(x => $"{x.Name}: {x.Description}"))}");
					throw;
				}
				catch (Exception ex)
				{
					_logger.Error(ex.Message, ex);
					throw;
				}
			}
		}

		public void Complete(IPurchaseOrder purchaseOrder)
		{
			if (purchaseOrder == null)
			{
				throw new ArgumentNullException(nameof(purchaseOrder));
			}
			var orderForm = purchaseOrder.GetFirstForm();
			var payment = orderForm?.Payments.FirstOrDefault(x => x.PaymentMethodName.Equals(Constants.PayExCheckoutSystemKeyword));
			if (payment == null)
			{
				return;
			}
		}

		public PaymentResponse GetPayment(string id, IOrderGroup cart, PaymentExpand paymentExpand = PaymentExpand.None)
		{
			var payExApi = GetPayExApi(cart);
			try
			{
				var paymentResponseObject = payExApi.PaymentOrders.GetPayment(id, paymentExpand);
				return paymentResponseObject;
			}
			catch (PayExException ex)
			{
				_logger.Error($"{ex.ErrorCode} - {ex.Error.Title}... {ex.Error.Detail}::: {string.Join(", ", ex.Error.Problems.Select(x => $"{x.Name}: {x.Description}"))}");
				throw;
			}
			catch (Exception ex)
			{
				_logger.Error(ex.Message, ex);
				throw;
			}
		}



		protected virtual PaymentOrderRequestObject GetCheckoutOrderData(
		  IOrderGroup orderGroup, IMarket market, PaymentMethodDto paymentMethodDto, string consumerProfileRef = null)
		{
			var totals = _orderGroupCalculator.GetOrderGroupTotals(orderGroup);
			var marketCountry = CountryCodeHelper.GetTwoLetterCountryCode(market.Countries.FirstOrDefault());
			if (string.IsNullOrWhiteSpace(marketCountry))
			{
				throw new ConfigurationException($"Please select a country in Commerce Manager for market {orderGroup.MarketId}");
			}
			var checkoutConfiguration = GetCheckoutConfiguration(market);


			var paymentOrderRequestObject = new PaymentOrderRequestObject
			{
				Paymentorder = new PaymentOrderRequest
				{
					Amount = AmountHelper.GetAmount(totals.Total),
					VatAmount = AmountHelper.GetAmount(totals.TaxTotal),
					Currency = market.DefaultCurrency.CurrencyCode,
					Description = "Description",
					Language = market.DefaultLanguage.TextInfo.CultureName,
					UserAgent = HttpContext.Current.Request.UserAgent,
					Urls = GetMerchantUrls(orderGroup),
					PayeeInfo = new PayeeInfo
					{
						PayeeId = checkoutConfiguration.MerchantId,
						PayeeReference = DateTime.Now.Ticks.ToString(),
					},
				}
			};

			if (!string.IsNullOrWhiteSpace(consumerProfileRef))
			{
				paymentOrderRequestObject.Paymentorder.Payer = new Payer
				{
					ConsumerProfileRef = consumerProfileRef
				};
			}

			return paymentOrderRequestObject;
		}

		protected virtual Urls GetMerchantUrls(IOrderGroup orderGroup)
		{
			if (PaymentMethodDto == null) return null;

			var configuration = GetConfiguration(orderGroup.MarketId);

			string ToFullSiteUrl(Func<CheckoutConfiguration, string> fieldSelector)
			{
				var url = fieldSelector(configuration).Replace("{orderGroupId}", orderGroup.OrderLink.OrderGroupId.ToString());
				if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
				{
					return uri.ToString();
				}

				return new Uri(SiteDefinition.Current.SiteUrl, url).ToString();
			}

			return new Urls
			{
				TermsOfServiceUrl = configuration.TermsOfServiceUrl,
				CallbackUrl = ToFullSiteUrl(c => c.CallbackUrl),
				CancelUrl = ToFullSiteUrl(c => c.CancelUrl),
				CompleteUrl = ToFullSiteUrl(c => c.CompleteUrl),
				LogoUrl = configuration.LogoUrl,
				HostUrls = configuration.HostUrls
			};
		}



		public CheckoutConfiguration GetConfiguration(IMarket market)
		{
			return _checkoutConfigurationLoader.GetConfiguration(market.MarketId, market.DefaultLanguage.Name);
		}

		private CheckoutConfiguration GetConfiguration(MarketId marketId)
		{
			return _checkoutConfigurationLoader.GetConfiguration(marketId);
		}

		private PayExApi GetPayExApi()
		{
			var market = _marketService.GetMarket(_currentMarket.GetCurrentMarket().MarketId);
			var checkoutConfiguration = GetCheckoutConfiguration(market);
			var payExApi = new PayExApi(checkoutConfiguration.ApiUrl, checkoutConfiguration.Token);
			return payExApi;
		}

		private PayExApi GetPayExApi(IOrderGroup orderGroup)
		{
			var market = _marketService.GetMarket(orderGroup.MarketId);
			var checkoutConfiguration = GetCheckoutConfiguration(market);
			var payExApi = new PayExApi(checkoutConfiguration.ApiUrl, checkoutConfiguration.Token);
			return payExApi;
		}
	}
}
