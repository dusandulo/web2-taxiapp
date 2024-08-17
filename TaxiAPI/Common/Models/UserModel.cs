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
        public Guid Id { get; set; }

        [DataMember]
        public string UserName { get; set; } = string.Empty;

        [DataMember]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [DataMember]
        public string Password { get; set; } = string.Empty;

        [DataMember]
        public string Name { get; set; } = string.Empty;

        [DataMember]
        public string LastName { get; set; } = string.Empty;

        [DataMember]
        public string Birthday { get; set; } = string.Empty;

        [DataMember]
        public string Address { get; set; } = string.Empty;

        [DataMember]
        public UserRole Role { get; set; }

        [DataMember]
        public string Image { get; set; } = string.Empty;

        [DataMember]
        public VerificationState VerificationState { get; set; } = VerificationState.Verified;
    }
}