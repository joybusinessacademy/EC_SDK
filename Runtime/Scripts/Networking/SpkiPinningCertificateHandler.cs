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
            "N3Yf3nUeQjP59CsBqAgnM8tzD7jssuTmZv5RyJlVJy4=", // api-anz-dev.skillsvr.com
            "N3Yf3nUeQjP59CsBqAgnM8tzD7jssuTmZv5RyJlVJy4=", // cdn-dev.skillsvr.com
            "pt0boNroUbh6ZMPw19Eo43QyKWBSF546X5VW8OgGC/w=", // api-anz-test.skillsvr.com
            "pt0boNroUbh6ZMPw19Eo43QyKWBSF546X5VW8OgGC/w=", // cdn-test.skillsvr.com
            "AkomCTcg9e8aBqcrUxbpRmwHtZR+CFNC+VM8UEfqGWs=", // api-us-stg.skillsvr.com
            "AkomCTcg9e8aBqcrUxbpRmwHtZR+CFNC+VM8UEfqGWs=", // cdn-stg.skillsvr.com
            "uGOlW6DZ0hejPXXdsGQTSYP71EgFS5wpNygAutUbARM=", // api-anz.skillsvr.com
            "uGOlW6DZ0hejPXXdsGQTSYP71EgFS5wpNygAutUbARM=", // api-us.skillsvr.com
            "uGOlW6DZ0hejPXXdsGQTSYP71EgFS5wpNygAutUbARM=", // cdn.skillsvr.com

            "N3Yf3nUeQjP59CsBqAgnM8tzD7jssuTmZv5RyJlVJy4=", // login.skvrentdevau.skillsvr.com
            "pt0boNroUbh6ZMPw19Eo43QyKWBSF546X5VW8OgGC/w=", // login.skvrenttestau.skillsvr.com
            "AkomCTcg9e8aBqcrUxbpRmwHtZR+CFNC+VM8UEfqGWs=", // login.skvrentstgus.skillsvr.com
            "uGOlW6DZ0hejPXXdsGQTSYP71EgFS5wpNygAutUbARM=", // login.skvrentprodus.skillsvr.com AND login.skvrentprodau.skillsvr.com
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