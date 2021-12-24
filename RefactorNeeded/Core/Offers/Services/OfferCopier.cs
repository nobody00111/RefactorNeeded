using System;
using System.Collections.Generic;
using System.Linq;
using RefactorNeeded.Commons.ErrorHandling;
using RefactorNeeded.Core.Customers;
using RefactorNeeded.Core.Offers.Entities;
using RefactorNeeded.Core.Offers.ValueObjects;
using RefactorNeeded.Core.PriceLists;
using RefactorNeeded.Core.Products;

namespace RefactorNeeded.Core.Offers.Services
{
    public class OfferCopier
    {
        private readonly Offer _offer;

        public OfferCopier(Offer offer)
        {
            _offer = offer;
        }
        
        public Either<Offer, DomainError> Copy(Customer customer, PriceList priceList, OfferNumber number, DateTime now,
            List<Product> products)
        {
            var offer = new Offer(customer, priceList, number, now)
            {
                CopiedFromOfferId = _offer.Id
            };

            foreach (var offerItem in _offer.Items)
            {
                CopyOfferItem(products, offerItem, offer);
            }

            foreach (var suggestedProduct in _offer.SuggestedProducts)
            {
                CopySuggestedProduct(products, suggestedProduct, offer);
            }

            return offer;
        }

        private void CopySuggestedProduct(List<Product> products, SuggestedProduct suggestedProduct, Offer offer)
        {
            var product = products.Single(x => x.Id == suggestedProduct.ProductId);

            var priceListPrice = product.GetPrice(_offer.PriceListId);

            if (priceListPrice == null) return;

            offer.SuggestProductInternal(product, priceListPrice.Price);
        }

        private void CopyOfferItem(List<Product> products, OfferItem offerItem, Offer offer)
        {
            var product = products.Single(x => x.Id == offerItem.ProductId);

            var priceListPrice = product.GetPrice(_offer.PriceListId);

            if (priceListPrice == null) return;

            var newOfferItem = offer.AddProductInternal(product, offerItem.Quantity, priceListPrice.Price);

            var supplementsAddedToProduct = _offer.GetSupplementsAddedToProduct(offerItem.ProductId);

            CopySupplements(offer, supplementsAddedToProduct, newOfferItem);
        }

        private static void CopySupplements(Offer offer, List<Supplement> supplementsAddedToProduct, OfferItem newOfferItem)
        {
            foreach (var supplement in supplementsAddedToProduct)
            {
                offer.AddSupplementInternal(newOfferItem, supplement.Name, supplement.Quantity,
                    supplement.UnitPrice);
            }
        }
    }
}