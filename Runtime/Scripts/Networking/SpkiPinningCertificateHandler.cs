using Org.BouncyCastle.X509;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.Networking;

public class SpkiPinningCertificateHandler : CertificateHandler
{
    private readonly IReadOnlyList<string> TRUSTED_CERTIFICATES = new List<string>()
        {
            "8V7EJHIMZAnlDhA/Jlu6MOtFEe6lrHZjw3lLWV5b4SI=", // api-anz-dev.skillsvr.com
            "H3fOLhh9YUTrZAa9NCdzLhc7a3cN1GEPTYdIVq2uK1U=", // cdn-dev.skillsvr.com
            "FgUxlJIJ9nTVizAzfo/z9IeZdgPxvm8dybyGaR90BLk=", // api-anz-test.skillsvr.com
            "SDVKAL1e/zQxBR65Nhz1WYi36ocsjmRx0ODLPQ507ks=", // cdn-test.skillsvr.com
            "NDN17RR65/SYNE0xF1WNirNg9/VUgPTYQZ29PzUV3U0=", // api-us-stg.skillsvr.com
            "ebqeRWOm3FrP/4sHnUJKaY5pDPqqKDGLlD+5IkgTiMc=", // cdn-stg.skillsvr.com
            "HPZVtCDvPDrkcz4+FvjEv9Nx3E7LBOrzPlXNiJ4Dnt8=", // api-anz.skillsvr.com
            "ELypq8xRNetvJtFhNSXi4D7xVTPHY1jACVV7lkzmZ0w=", // api-us.skillsvr.com
            "fe+gBi6XCE/LfVelTQirCRBE4/l1TvEDI6gDSMpqWcI=", // cdn.skillsvr.com

            "q9qG38hki2DExniBcrlowEs8virMP2Fu/zhjiKUoqLA=", // login.skvrentdevau.skillsvr.com
            "/toC8a6Grx9nHbgqlVN2RcHF0yVar4fMaVNgTpRQ6a8=", // login.skvrenttestau.skillsvr.com
            "Dx0YvS9SOno4Yyfp2XW6iT1CjDBME/yiXwtDxNDAu8o=", // login.skvrentstgus.skillsvr.com
            "Uia3B59AJecB4y8wpRkfH+vjAQR5ofnY2O9uWvzLJp8=", // login.skvrentprodus.skillsvr.com AND login.skvrentprodau.skillsvr.com
        };

    protected override bool ValidateCertificate(byte[] certificateData)
    {
        try
        {
            // Convert to X509Certificate2
            var cert = new X509Certificate2(certificateData);

            // Parse with BouncyCastle
            var bcCert = new X509CertificateParser().ReadCertificate(cert.RawData);
            var spki = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(bcCert.GetPublicKey());
            var spkiBytes = spki.GetDerEncoded();

            // Hash
            using var sha256 = SHA256.Create();
            var spkiHash = sha256.ComputeHash(spkiBytes);
            var spkiBase64 = Convert.ToBase64String(spkiHash);

            UnityEngine.Debug.Log($"[SPKI] Remote cert SPKI: {spkiBase64}");

            return TRUSTED_CERTIFICATES.Contains(spkiBase64);
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"SPKI pinning failed: {ex}");
            return false;
        }
    }

    private string GetCdnForUrl(string url)
    {
        string hostName = new Uri(url).Host;
        UriBuilder uriBuilder = new UriBuilder(url);
        if (hostName.Contains("test"))
        {
            uriBuilder.Host = "cdn-test.skillsvr.com";
        }
        else if (hostName.Contains("dev"))
        {
            uriBuilder.Host = "cdn-dev.skillsvr.com";
        }
        else if (hostName.Contains("stg"))
        {
            uriBuilder.Host = "cdn-stg.skillsvr.com";
        }
        else
        {
            uriBuilder.Host = "cdn.skillsvr.com";
        }
        return uriBuilder.ToString();
    }
}