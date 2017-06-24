using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Reta.Models
{
    public class SmtpConfigurationUser
    {
        [Key]
        public string id { get; set; }
        [Display(Name = "Serveur (ex : ssl0.ovh.net)")]
        public string host { get; set; }
        [Display(Name = "Port")]
        public string port { get; set; }
        [Display(Name = "Identifiant (généralement adresse email)")]
        public string username { get; set; }
        [Display(Name = "Mot de passe de la boîte email")]
        public string password { get; set; }
        [Display(Name = "Activer le protocole SSL")]
        public string enableSsl { get; set; }
        [Display(Name = "Temps d'expiration de la requête")]
        public string timeout { get; set; }
    }
}