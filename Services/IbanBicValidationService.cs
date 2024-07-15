using SinKien.IBAN4Net;
using SinKien.IBAN4Net.Exceptions;

namespace VendorAPI.Services;

public class IbanBicValidationService
{
    public (bool IsValid, string ErrorMessage) ValidateIban(string iban)
    {
        if (string.IsNullOrEmpty(iban))
        {
            return (false, "IBAN is required.");
        }

        try
        {
            IbanUtils.Validate(iban);
            return (true, string.Empty);
        }
        catch (IbanFormatException ex)
        {
            return (false, $"Invalid IBAN format: {ex.Message}");
        }
        catch (InvalidCheckDigitException ex)
        {
            return (false, $"Invalid IBAN check digits: {ex.Message}");
        }
        catch (Exception)
        {
            return (false, "An error occurred while validating the IBAN.");
        }
    }

    public (bool IsValid, string ErrorMessage) ValidateBic(string bic)
    {
        if (string.IsNullOrEmpty(bic))
        {
            return (false, "BIC is required.");
        }

        try
        {
            BicUtils.ValidateBIC(bic);
            return (true, string.Empty);
        }
        catch (BicFormatException ex)
        {
            return (false, $"Invalid IBAN format: {ex.Message}");
        }
        catch (Exception)
        {
            return (false, "An error occurred while validating the IBAN.");
        }
    }
}