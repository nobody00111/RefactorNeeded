using System;
using System.Collections.Generic;
using System.Linq;
using RefactorNeeded.Commons;
using RefactorNeeded.Commons.ErrorHandling;
using RefactorNeeded.Commons.Extensions;
using RefactorNeeded.Commons.ValueObjects;
using RefactorNeeded.Core.Customers;
using RefactorNeeded.Core.Offers.Config;
using RefactorNeeded.Core.Offers.Enums;
using RefactorNeeded.Core.Offers.Services;
using RefactorNeeded.Core.Offers.ValueObjects;
using RefactorNeeded.Core.PriceLists;
using RefactorNeeded.Core.Products;

namespace RefactorNeeded.Core.Offers.Entities
{
    public class Offer : Entity
    {
        private const int OfferValidityInDays = 30;
        
        public OfferId Id { get; }

        public CustomerId CustomerId { get; }

        public OfferNumber Number { get; }

        public OfferStatus Status { get; private set; }

        public Money TotalPrice { get; private set; }

        public Money TotalPriceAfterDiscount { get; private set; }

        public PriceListId PriceListId { get; }

        public Currency Currency { get; }

        public DateTime CreationDateTime { get; }

        public Email? SentTo { get; private set; }

        public DateTime? SentAt { get; private set; }

        public DateTime? AcceptedAt { get; private set; }

        public string? CancellationReason { get; private set; }

        public DateTime? CancelledAt { get; private set; }

        public string? RejectionReason { get; private set; }

        public DateTime? RejectedAt { get; private set; }

        public DiscountStatus DiscountStatus { get; private set; }

        public string? DiscountApprovalRequestMessage { get; private set; }

        public string? DiscountRejectionReason { get; private set; }

        public OfferId? CopiedFromOfferId { get; internal set; }

        private List<OfferItem> _items = new();

        public IReadOnlyCollection<OfferItem> Items
        {
            get => _items.AsReadOnly();
            private set => _items = value.ToList();
        }

        private List<SuggestedProduct> _suggestedProducts = new();

        public IReadOnlyCollection<SuggestedProduct> SuggestedProducts
        {
            get => _suggestedProducts.AsReadOnly();
            private set => _suggestedProducts = value.ToList();
        }

        private List<Supplement> _supplements = new();

        public IReadOnlyCollection<Supplement> Supplements
        {
            get => _supplements.AsReadOnly();
            private set => _supplements = value.ToList();
        }

        #nullable disable
        private Offer()
        {
        }
        #nullable restore

        public Offer(Customer customer, PriceList priceList, OfferNumber number, DateTime now)
        {
            Id = new OfferId(Guid.NewGuid());
            CustomerId = customer.Id;
            PriceListId = priceList.Id!;
            Currency = customer.Currency;
            Number = number;
            CreationDateTime = now;
            Status = OfferStatus.New;
            TotalPrice = Money.Zero(customer.Currency);
            TotalPriceAfterDiscount = Money.Zero(customer.Currency);
        }

        public Either<Offer, DomainError> Copy(Customer customer, PriceList priceList, OfferNumber number, DateTime now,
            List<Product> products)
        {
            return new OfferCopier(this).Copy(customer, priceList, number, now, products);
        }

        public Either<Success, DomainError> RequestDiscountApproval(string message)
        {
            if (DiscountStatus != DiscountStatus.ThresholdExceeded)
                return new DomainError("offers.request_discount_approval.wrong_discount_status",
                    "Wrong discount status.");

            DiscountStatus = DiscountStatus.SentForApproval;
            DiscountApprovalRequestMessage = message;

            return Success.Value;
        }

        public Either<Success, DomainError> ApproveDiscount()
        {
            if (DiscountStatus != DiscountStatus.SentForApproval)
                return new DomainError("offers.approve_discount.wrong_discount_status", "Wrong discount status.");

            DiscountStatus = DiscountStatus.Approved;

            foreach (var item in _items) item.ApprovePrice();

            return Success.Value;
        }

        public Either<Success, DomainError> RejectDiscount(string reason)
        {
            if (DiscountStatus != DiscountStatus.SentForApproval)
                return new DomainError("offers.reject_discount.wrong_discount_status", "Wrong discount status.");

            DiscountStatus = DiscountStatus.Rejected;
            DiscountRejectionReason = reason;

            return Success.Value;
        }

        private void RefreshDiscountStatus()
        {
            if (HasDiscountThresholdBeenExceeded())
            {
                if (DiscountStatus != DiscountStatus.SentForApproval) DiscountStatus = DiscountStatus.ThresholdExceeded;
            }
            else
            {
                DiscountStatus = DiscountStatus.ThresholdNotExceeded;
            }
        }

        private bool HasDiscountThresholdBeenExceeded()
        {
            return _items.Any(x => x.IsDiscountThresholdExceeded);
        }

        public Either<Success, DomainError> AddProductFromPriceList(Product product, Quantity quantity)
        {
            if (Status != OfferStatus.New)
                return new DomainError("offers.add_product.offer_cannot_be_modified", "Offer cannot be modified.");

            if (GetItemByProductId(product.Id) != null)
                return new DomainError("offers.add_product.product_already_added",
                    "Product has already been added to the offer.");

            var priceListPrice = product.GetPrice(PriceListId);

            if (priceListPrice == null)
                return new DomainError("offers.add_product.product_not_added_to_price_list",
                    "Product is not added to price list.");

            AddProductInternal(product, quantity, priceListPrice.Price);

            return Success.Value;
        }

        internal OfferItem AddProductInternal(Product product, Quantity quantity, Money price)
        {
            var minimalPrice = product.GetProductMinimalPrice(DiscountThresholdConfig.Default);

            var newItem = new OfferItem(product, quantity, price, minimalPrice.Value);
            _items.Add(newItem);

            RefreshPrices();
            RefreshDiscountStatus();

            return newItem;
        }

        public Either<Success, DomainError> RemoveProduct(ProductId productId)
        {
            if (Status != OfferStatus.New)
                return new DomainError("offers.remove_product.offer_cannot_be_modified", "Offer cannot be modified.");

            var item = GetItemByProductId(productId);

            if (item == null)
                return new DomainError("offers.remove_product.product_not_added", "Cannot find offer item.");

            RemoveProductInternal(item);

            return Success.Value;
        }

        private void RemoveProductInternal(OfferItem item)
        {
            _items.Remove(item);

            RefreshPrices();
            RefreshDiscountStatus();
        }

        public Either<Success, DomainError> ChangeProductPrice(ProductId productId, Money price)
        {
            if (Status != OfferStatus.New)
                return new DomainError("offers.change_product_price.offer_cannot_be_modified", "Offer cannot be modified.");

            if (price.Currency != Currency)
                return new DomainError("offers.change_product_price.wrong_currency", "Currency is wrong.");

            var item = GetItemByProductId(productId);

            if (item == null)
                return new DomainError("offers.change_product_price.product_not_added", "Cannot find offer item.");

            item.SetUnitPrice(price);

            RefreshPrices();
            RefreshDiscountStatus();

            return Success.Value;
        }

        public Either<Success, DomainError> ChangeProductQuantity(ProductId productId, Quantity quantity)
        {
            if (Status != OfferStatus.New)
                return new DomainError("offers.change_product_quantity.offer_cannot_be_modified", "Offer cannot be modified.");

            var item = GetItemByProductId(productId);

            if (item == null)
                return new DomainError("offers.change_product_quantity.product_not_added", "Cannot find offer item.");

            item.SetQuantity(quantity);

            var supplementsAddedToProduct = GetSupplementsAddedToProduct(item.ProductId);

            foreach (var supplement in supplementsAddedToProduct)
                if (!supplement.IsQuantityFixed)
                    supplement.ChangeQuantity(quantity);

            RefreshPrices();

            return Success.Value;
        }

        public Either<Success, DomainError> SetDiscount(ProductId productId, Discount discount,
            CustomerMaxDiscount customerMaxDiscount)
        {
            var item = GetItemByProductId(productId);

            if (item == null)
                return new DomainError("offers.set_discount.product_not_added", "Cannot find offer item.");

            var discountValidationResult = item.ValidateDiscount(discount);

            if (!discountValidationResult.IsSuccess) return discountValidationResult.Error;

            if (customerMaxDiscount.IsExceeded(item.UnitPrice, item.CalculateUnitPriceAfterDiscount(discount)))
                return new DomainError("offers.set_discount.customer_max_discount_exceeded",
                    "Customer max discount was exceeded.");

            item.SetDiscount(discount);

            RefreshPrices();

            return Success.Value;
        }

        private bool IsProductAdded(ProductId productId)
        {
            return GetItemByProductId(productId) != null;
        }

        private OfferItem? GetItemByProductId(ProductId productId)
        {
            return _items.SingleOrDefault(x => x.ProductId == productId);
        }

        private void RefreshPrices()
        {
            TotalPrice = CalculateTotalPrice();
            TotalPriceAfterDiscount = CalculateTotalPriceAfterDiscount();
        }

        private Money CalculateTotalPrice()
        {
            var totalItemPrice = CalculateTotalItemPrice();

            var totalSupplementPrice = CalculateTotalSupplementPrice();

            return totalItemPrice.Add(totalSupplementPrice);
        }

        private Money CalculateTotalPriceAfterDiscount()
        {
            var totalItemPriceAfterDiscount = CalculateTotalItemPriceAfterDiscount();

            var totalSupplementPrice = CalculateTotalSupplementPrice();

            return totalItemPriceAfterDiscount.Add(totalSupplementPrice);
        }

        private Money CalculateTotalItemPrice()
        {
            return _items.Aggregate(Money.Zero(Currency), (total, item) => total.Add(item.TotalUnitPrice));
        }

        private Money CalculateTotalItemPriceAfterDiscount()
        {
            return _items.Aggregate(Money.Zero(Currency), (total, item) => total.Add(item.TotalUnitPriceAfterDiscount));
        }

        private Money CalculateTotalSupplementPrice()
        {
            return _supplements.Aggregate(Money.Zero(Currency),
                (total, supplement) => total.Add(supplement.TotalPrice));
        }

        public Either<Success, DomainError> Send(Email email, DateTime now)
        {
            if (Status != OfferStatus.New)
                return new DomainError("offers.send_offer.wrong_status", "Offer must have status: New.");

            if (!Items.Any())
                return new DomainError("offers.send_offer.no_products_selected", "Offer has no selected products.");

            if ((now - CreationDateTime).Days > OfferValidityInDays)
                return new DomainError("offers.send_offer.offer_too_old", "Offer has more than 30 days.");

            if (DiscountStatus.In(
                DiscountStatus.ThresholdExceeded,
                DiscountStatus.Rejected,
                DiscountStatus.SentForApproval))
                return new DomainError("offers.send_offer.wrong_discount_status", "Discount status is not correct.");

            Status = OfferStatus.Sent;
            SentTo = email;
            SentAt = now;

            return Success.Value;
        }

        public Either<Success, DomainError> Cancel(string reason, DateTime now)
        {
            if (Status != OfferStatus.New)
                return new DomainError("offers.cancel_offer.wrong_status", "Offer must have status: New.");

            Status = OfferStatus.Cancelled;
            CancellationReason = reason;
            CancelledAt = now;

            return Success.Value;
        }

        public Either<Success, DomainError> Accept(DateTime now)
        {
            if (Status != OfferStatus.Sent)
                return new DomainError("offers.accept_offer.wrong_status", "Offer must have status: Sent.");

            Status = OfferStatus.Accepted;
            AcceptedAt = now;

            return Success.Value;
        }

        public Either<Success, DomainError> Reject(string reason, DateTime now)
        {
            if (Status != OfferStatus.Sent)
                return new DomainError("offers.reject_offer.wrong_status", "Offer must have status: Sent.");

            Status = OfferStatus.Rejected;
            RejectionReason = reason;
            RejectedAt = now;

            return Success.Value;
        }

        public Either<Success, DomainError> SuggestProduct(Product product)
        {
            if (Status != OfferStatus.New)
                return new DomainError("offers.suggest_product.offer_cannot_be_modified", "Offer cannot be modified.");

            if (IsProductSuggested(product.Id))
                return new DomainError("offers.suggest_product.product_already_suggested",
                    "Product has already been suggested.");

            if (IsProductAdded(product.Id))
                return new DomainError("offers.suggest_product.product_already_added",
                    "Product has already been added to offer.");

            var priceListPrice = product.GetPrice(PriceListId);

            if (priceListPrice == null)
                return new DomainError("offers.suggest_product.product_not_added_to_price_list",
                    "Product is not added to price list.");

            SuggestProductInternal(product, priceListPrice.Price);

            return Success.Value;
        }

        internal void SuggestProductInternal(Product product, Money price)
        {
            var newSuggestedProduct = new SuggestedProduct(product.Id, price);

            _suggestedProducts.Add(newSuggestedProduct);
        }

        public Either<Success, DomainError> RemoveSuggestedProduct(ProductId productId)
        {
            if (Status != OfferStatus.New)
                return new DomainError("offers.remove_suggested_product.offer_cannot_be_modified", "Offer cannot be modified.");

            var suggestedProduct = GetSuggestedProduct(productId);

            if (suggestedProduct == null)
                return new DomainError("offers.remove_suggested_product.product_not_suggested",
                    "Cannot find suggested product.");

            _suggestedProducts.Remove(suggestedProduct);

            return Success.Value;
        }

        public Either<Success, DomainError> MoveToSuggestedProducts(ProductId productId)
        {
            if (Status != OfferStatus.New)
                return new DomainError("offers.move_to_suggested_products.offer_cannot_be_modified", "Offer cannot be modified.");

            var item = GetItemByProductId(productId);

            if (item == null)
                return new DomainError("offers.move_to_suggested_products.product_not_added",
                    "Cannot find offer item.");

            RemoveProductInternal(item);

            var newSuggestedProduct = new SuggestedProduct(item.ProductId, item.UnitPrice);

            _suggestedProducts.Add(newSuggestedProduct);

            return Success.Value;
        }

        public Either<Success, DomainError> AddFromSuggestedProducts(Product product)
        {
            var suggestedProduct = GetSuggestedProduct(product.Id);

            if (suggestedProduct == null)
                return new DomainError("offers.add_from_suggested_products.product_not_suggested",
                    "Product has not been suggested.");

            _suggestedProducts.Remove(suggestedProduct);

            AddProductInternal(product, Quantity.Single, suggestedProduct.Price);

            return Success.Value;
        }

        private bool IsProductSuggested(ProductId productId)
        {
            return GetSuggestedProduct(productId) != null;
        }

        private SuggestedProduct? GetSuggestedProduct(ProductId productId)
        {
            return _suggestedProducts.SingleOrDefault(x => x.ProductId == productId);
        }

        public Either<Supplement, DomainError> AddSupplement(ProductId productId, string name, Quantity? quantity,
            Money unitPrice)
        {
            var item = GetItemByProductId(productId);

            if (item == null)
                return new DomainError("offers.add_supplement.product_not_added", "Cannot find offer item.");

            var newSupplement = AddSupplementInternal(item, name, quantity, unitPrice);

            return newSupplement;
        }

        internal Supplement AddSupplementInternal(OfferItem item, string name, Quantity? quantity, Money unitPrice)
        {
            var newSupplement = new Supplement(item, name, quantity, unitPrice);

            _supplements.Add(newSupplement);

            RefreshPrices();

            return newSupplement;
        }

        internal List<Supplement> GetSupplementsAddedToProduct(ProductId productId)
        {
            return _supplements.Where(x => x.ProductId == productId).ToList();
        }
    }
}