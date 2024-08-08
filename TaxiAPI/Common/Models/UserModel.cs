using Common.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Common.Models
{
    [DataContract]
    public class UserModel
    {
        [DataMember]
        [Required]
        public Guid Id { get; set; }

        [DataMember]
        [Required]
        public string UserName { get; set; } = string.Empty;

        [DataMember]
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [DataMember]
        [Required]
        public string Password { get; set; } = string.Empty;

        [DataMember]
        [Required]
        public string Name { get; set; } = string.Empty;

        [DataMember]
        [Required]
        public string LastName { get; set; } = string.Empty;

        [DataMember]
        [Required]
        public string Birthday { get; set; } = string.Empty;

        [DataMember]
        [Required]
        public string Address { get; set; } = string.Empty;

        [DataMember]
        [Required]
        public UserRole Role { get; set; }

        [DataMember]
        public string Image { get; set; } = string.Empty;

        [DataMember]
        [Required]
        public VerificationState VerificationState { get; set; } = VerificationState.Verified;
    }
}