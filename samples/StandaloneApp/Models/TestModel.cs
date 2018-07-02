using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace StandaloneApp.Models
{
    public class TestModel
    {
        [Required]
        [MaxLength(255)]
        [Display(Name = "Text field")]
        public string TextValue { get; set; }

        [Required]
        [Display(Name = "Int field")]
        public int IntValue { get; set; }

        [Display(Name = "Nullable Int field")]
        public int? NullableIntValue { get; set; }

        [Required]
        [Display(Name = "Float field")]
        public float FloatValue { get; set; }

        [Required]
        [Display(Name = "Double field")]
        public double DoubleValue { get; set; }

        [Required]
        [Display(Name = "DateTime field")]
        public System.DateTime DateTimeValue { get; set; }

        [Display(Name = "Nullable DateTime field")]
        public System.DateTime? NullableDateTimeValue { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email field")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Test for combo (int)")]
        public int ComboTestInt { get; set; }

        [Required]
        [Display(Name = "Test for combo (string)")]
        public string ComboTestString { get; set; }

        [Required]
        [Display(Name = "Test for combo (enum)")]
        public TestEnum ComboTestEnum { get; set; }
    }

    public enum TestEnum
    {
        Value1,
        Value2,
        Value3
    }
}