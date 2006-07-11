/***************************************************************************
 *  CreativeCommons.cs
 *
 *  Copyright (C) 2006 Luke Hoersten
 *  Written by Luke Hoersten <luke.hoersten@gmail.com>
 ****************************************************************************/

using System;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Text;
using System.Security.Cryptography;

using Banshee.Base;
using CCLicenseLib.Utilities;

namespace Banshee.Base
{
	public static class CreativeCommons
	{
	    private const string HTTP_STRING = "http://";
		private const string VERIFY_STRING = "verify at ";
		private const int VERIFY_LENGTH = 10;

		public static void VerifyLicense(TrackInfo track)
		{
		    string license_uri;
            string metadata_uri;
        	
	        try {
                ParseClaim(track, out license_uri, out metadata_uri);
	        } catch(ArgumentException e) {
                Console.WriteLine(e);
                return;    
	        }

			RDFParser metadata_parser = new RDFParser(metadata_uri, true);
			if(LicenseInMetadata(license_uri,
					HexDigest(track.Uri.AbsolutePath),
					metadata_parser.GetRDFAsString())) {
				SetLicense(track, license_uri);
			}
		}
		
		private static void SetLicense(TrackInfo track, string license_uri) {
			track.License = GetLicenseName(license_uri);
		}
		
		private static void ParseClaim(TrackInfo track, out string license_uri, out string metadata_uri)
		{
		    /* WCOP/WOAF and TCOP parsing */
	    	if((track.LicenseUri != null) &&
			   (track.MetadataUri != null)) {
				license_uri = track.LicenseUri;
			    metadata_uri = track.MetadataUri;
			} else if(track.Copyright != null) {
				license_uri = ParseLicenseUri(track.Copyright);
			    metadata_uri = ParseMetadataUri(track.Copyright);
			} else {
				throw new ArgumentException("No license claim was made.");
			}
		}
		
		private static string HexDigest(string file_path) {
			Stream stream = new FileStream(file_path, FileMode.Open, FileAccess.Read);

			SHA1 sha1_crypto = new SHA1CryptoServiceProvider();
			byte[] digest = sha1_crypto.ComputeHash(stream);
			string hex_digest = "";
			foreach (byte i in digest) {
				hex_digest = hex_digest + i.ToString("x2");
			}
		
			Console.WriteLine("File: \"{0}\" Hex Digest: \"{1}\"", file_path, hex_digest);
//			SHA1Managed hasher = new SHA1Managed();
//			string file_hash = Convert.ToBase64String(
//									hasher.ComputeHash(File.OpenRead(file_path)));
//			Console.WriteLine("File: \"{0}\" Hash: \"{1}\"", file_path, file_hash);
			return hex_digest.ToUpper();
		}
		
		private static bool LicenseInMetadata(string license_uri, string track_hash, string metadata)
		{
			XPathDocument doc = new XPathDocument(metadata);
       		XPathNavigator navigator = doc.CreateNavigator();
       		XPathExpression expression = navigator.Compile(
       			String.Format("/rdf:RDF/r:Work[@rdf:about='urn:sha1:{0}']/r:license/@rdf:resource", track_hash));
       		
       		XmlNamespaceManager namespaces = new XmlNamespaceManager(new NameTable());
       		namespaces.AddNamespace ("rdf", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
       		namespaces.AddNamespace ("dc", "http://purl.org/dc/elements/1.1/");
       		namespaces.AddNamespace ("r", "http://web.resource.org/cc/");
       		expression.SetContext(namespaces);
       		
       		XPathNodeIterator it = navigator.Select(expression);
       		Console.WriteLine ("Found {0} license match(es) in metadata.", it.Count);
       		while (it.MoveNext()) {
       			XPathNavigator n = it.Current;
       			if(n.Value == license_uri) {
       				return true;
       			}
       		}
       		return false;
		}

        // TODO: VERIFY_STRING is being searched twice.
		private static string ParseLicenseUri(string data)
		{
		    int verify_index = (data.ToLower()).IndexOf(VERIFY_STRING);
		    if(verify_index <= 0) {
                throw new ArgumentException("No metadata was found in Copyright tag at parsing license url");    
		    }
		    
		    int http_index = (data.ToLower()).LastIndexOf(HTTP_STRING, verify_index);
		    if(http_index <= 0) {
                throw new ArgumentException("No license was found in Copyright tag");
		    }
		    
			return data.Substring(http_index, (verify_index - http_index) - 1);
		}
		
		private static string ParseMetadataUri(string data)
		{
			int verify_index = data.ToLower().IndexOf(VERIFY_STRING);
			if(verify_index <= 0) {
			    throw new ArgumentException("No metadata was found in Copyright tag at parsing metadata url");
			}

			return data.Substring(verify_index + VERIFY_LENGTH, data.Length - (verify_index + VERIFY_LENGTH));
		}
				
		private static string GetLicenseName(string rdf)
		{
			return null;
		}
	}
}
