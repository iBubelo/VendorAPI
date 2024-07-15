using PhoneNumbers;

namespace VendorAPI.Services;

public partial class PhoneValidationService
{
    private readonly PhoneNumberUtil _phoneUtil;

    public PhoneValidationService()
    {
        _phoneUtil = PhoneNumberUtil.GetInstance();
    }

    public (bool IsValid, string ErrorMessage) ValidatePhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrEmpty(phoneNumber))
        {
            return (false, "Phone number is required.");
        }

        if (!phoneNumber.StartsWith('+'))
        {
            return (false, "Phone should start with a plus sign.");
        }

        try
        {
            var number = _phoneUtil.Parse(phoneNumber, "ZZ");
            var isValid = _phoneUtil.IsValidNumber(number);
            return (isValid, string.Empty);
        }
        catch (NumberParseException ex)
        {
            return (false, $"Invalid phone number: {ex.Message}");
        }
        catch (Exception)
        {
            return (false, "An error occurred while validating the phone number.");
        }
    }
}