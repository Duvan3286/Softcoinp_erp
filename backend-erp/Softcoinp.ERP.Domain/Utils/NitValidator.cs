using System;
using System.Linq;

namespace Softcoinp.ERP.Domain.Utils;

public static class NitValidator
{
    /// <summary>
    /// Calcula el dígito de verificación para un NIT colombiano.
    /// Utiliza el algoritmo del módulo 11 estandarizado por la DIAN.
    /// </summary>
    /// <param name="nit">NIT como string (solo números)</param>
    /// <returns>Dígito de verificación como string</returns>
    public static string CalculateVerificationDigit(string nit)
    {
        if (string.IsNullOrWhiteSpace(nit) || !nit.All(char.IsDigit))
        {
            throw new ArgumentException("El NIT debe contener únicamente dígitos numéricos.");
        }

        int[] primes = { 3, 7, 13, 17, 19, 23, 29, 37, 41, 43, 47, 53, 59, 67, 71 };
        int sum = 0;

        for (int i = 0; i < nit.Length; i++)
        {
            int digit = int.Parse(nit.Substring(nit.Length - 1 - i, 1));
            sum += digit * primes[i];
        }

        int remainder = sum % 11;
        
        if (remainder > 1)
        {
            return (11 - remainder).ToString();
        }
        else
        {
            return remainder.ToString();
        }
    }

    /// <summary>
    /// Valida si el dígito de verificación proporcionado coincide con el calculado para el NIT.
    /// </summary>
    public static bool IsValid(string nit, string verificationDigit)
    {
        try
        {
            string calculatedDigit = CalculateVerificationDigit(nit);
            return calculatedDigit == verificationDigit;
        }
        catch
        {
            return false;
        }
    }
}
