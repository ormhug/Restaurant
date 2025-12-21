using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Domain.Interfaces; 

namespace Domain.Entities
{
    public class MenuItem : IItemValidating
    {
        


        public string ImagePath { get; set; } = string.Empty;




        public Guid Id { get; set; }

        public string UniqueImportId { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Currency { get; set; } = "EUR";

        public int RestaurantId { get; set; }
        public Restaurant? Restaurant { get; set; } 

        public List<string> GetValidators()
        {
            if (Restaurant != null && !string.IsNullOrEmpty(Restaurant.OwnerEmailAddress))
            {
                return new List<string> { Restaurant.OwnerEmailAddress };
            }
            return new List<string>();
        }

        public string GetCardPartial()
        {
            return "_MenuItemCardPartial";
        }



        public List<string> Validate()
        {
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(Title))
            {
                errors.Add("Menu Item Title cannot be empty.");
            }
            if (Price <= 0)
            {
                errors.Add("Price must be greater than zero.");
            }

            return errors;
        }

        public string GetId()
        {
            return Id.ToString();
        }
    }
}