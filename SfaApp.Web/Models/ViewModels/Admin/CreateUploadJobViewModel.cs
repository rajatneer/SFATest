using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace SfaApp.Web.Models.ViewModels.Admin;

public class CreateUploadJobViewModel
{
    [Required]
    [StringLength(50)]
    public string UploadType { get; set; } = "Customers";

    [Required]
    public IFormFile? UploadFile { get; set; }
}