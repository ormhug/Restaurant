using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Interfaces; 

namespace Domain.Entities
{
    public class Restaurant : IItemValidating
    {
        public int Id { get; set; }

        public string UniqueImportId { get; set; } = string.Empty;

        public string ImagePath { get; set; } = string.Empty;


        public string Status { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string OwnerEmailAddress { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;

        public ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();

        public List<string> GetValidators()
        {
            // МЕЙЛ АДМИНА САЙТА
            return new List<string> { "admin@test.com" };
        }

        public string GetCardPartial()
        {

            return "_RestaurantCardPartial";
        }

        //public List<string> Validate()
        //{
        //    throw new NotImplementedException();
        //}

        public List<string> Validate()
        {
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(Name))
            {
                errors.Add("Restaurant Name cannot be empty.");
            }
            if (string.IsNullOrWhiteSpace(OwnerEmailAddress))
            {
                errors.Add("Owner Email Address cannot be empty.");
            }

            return errors;
        }

        public string GetId()
        {
            return Id.ToString();
        }

    }
}