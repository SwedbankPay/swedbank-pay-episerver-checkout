using System;
using System.Linq;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using Mediachase.Commerce.Catalog;
using Mediachase.MetaDataPlus.Configurator;

namespace SwedbankPay.Episerver.Checkout.Common
{
    using MetaClass = Mediachase.MetaDataPlus.Configurator.MetaClass;
    using MetaField = Mediachase.MetaDataPlus.Configurator.MetaField;

    [InitializableModule]
    public class OrderSystemInitalizationModule : IInitializableModule
    {
        public void Initialize(InitializationEngine context)
        {
            AddCustomProperty(Constants.SwedbankPayOrderIdField, "PurchaseOrder", "Mediachase.Commerce.Orders", MetaDataType.ShortString);
        }

        public void Uninitialize(InitializationEngine context)
        {
         
        }

        private static void AddCustomProperty(string propertyName, string metaClassName, string metaNamespace, MetaDataType metaDataType = MetaDataType.ShortString)
        {
            var metaField = GetMetaField(propertyName, metaDataType, metaNamespace);
            var metaClass = MetaClass.Load(CatalogContext.MetaDataContext, metaClassName);

            var currentFields = metaClass.GetUserMetaFields().ToList();
            if (currentFields.All(x => x.Name != propertyName))
            {
                metaClass.AddField(metaField);
            }
        }

        private static MetaField GetMetaField(string propertyName, MetaDataType metaDataType, string metaNamespace)
        {
            var metaField = MetaField.Load(CatalogContext.MetaDataContext, propertyName);
            var fieldExists = metaField != null;
            if (!fieldExists)
            {
                    metaField = MetaField.Create(
                        CatalogContext.MetaDataContext,
                        metaNamespace, // MetaNamespace
                        propertyName, // name
                        propertyName, // FriendlyName
                        $"Holds the {propertyName} of the line item", // Description
                        metaDataType, // Database type
                        Int32.MaxValue, // Databae length
                        true, // nullable
                        false, // multi language value
                        false, // allow search
                        false); // is encrypted				
            }

            return metaField;
        }
    }
}
