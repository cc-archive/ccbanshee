/***************************************************************************
 *  CreativeCommons.cs
 *
 *  Copyright (C) 2006 Luke Hoersten
 *  Written by Luke Hoersten <luke.hoersten@gmail.com>
 ****************************************************************************/

using System;
using System.IO;
using System.Web;
using System.Net;
using Banshee.Base;
using CCLicenseLib.Utilities;

namespace Banshee.Base
{
	public static class CreativeCommons
	{
	    private const string HTTP_STRING = "http:// ";
		private const string VERIFY_STRING = "verify at "; // language that denotes where to verify license
		private const int NUM_VERIFY_LENGTH = 10;          // length to skip to get the uri

		public static string VerifyLicense(TrackInfo track)
		{
	        string license_uri;    // License URI
	        string metadata_uri;   // URI of page containing RDF metadata that verifies a license URI
	        
	        // MP3 new and old license methods
	        try {
    			if((track.LicenseURI != null) &&
    			   (track.MetadataURI != null)) { // WCOP/WOAF method
    			    Console.WriteLine("== WCOP ==");
    				license_uri = track.LicenseURI;
    			    metadata_uri = track.MetadataURI;
    			} else if(track.Copyright != null) { // TCOP method (depricated)
    			    Console.WriteLine("== TCOP ==");
    				license_uri = ParseLicenseURI(track.Copyright);
    			    metadata_uri = ParseMetadataURI(track.Copyright);
    			} else { // No claim
    				throw new ArgumentException("No license claim was made.");
    			}
	        } catch(ArgumentException ex) {
	            Console.WriteLine("== CC Error ==");
                Console.WriteLine(ex);
                return null;    
	        }
						
			RDFParser metadata_parser = new RDFParser(metadata_uri, true);
			Console.WriteLine(metadata_parser.GetRDFAsString());
			
			/*int license_hash = license_uri.GetHashCode();
			int metadata_hash = (ExtractLicenseURI(metadata_parser.GetRDFAsString())).GetHashCode();
			
			if(license_hash == metadata_hash) {
				return GetLicenseName(metadata_parser.GetRDFAsString());
			} else {
				// Use Banshee log to record the error
				return "False Claim";
			}*/
			return "hey";
		}
		
		// Return a license name based on the license URI using CCLicenseLib
		private static string GetLicenseName(string rdf)
		{
			return null;
		}
		
		// Extract the license URI from the rdf tag
		private static string ExtractLicenseURI(string rdf)
		{
			return null;
		}

        // TODO: VERIFY_STRING is being indexed twice. This is bad.

		// Extract the license URI from TCOP frame	
		private static string ParseLicenseURI(string data)
		{
		    int verify_index = (data.ToLower()).IndexOf("verify at ");
		    if(verify_index <= 0) {
                throw new ArgumentException("No metadata was found in Copyright tag at parsing license url");    
		    }
		    
		    int http_index = (data.ToLower()).LastIndexOf("http://", verify_index);
		    if(http_index <= 0) {
                throw new ArgumentException("No license was found in Copyright tag");
		    }
		    
		    string temp = data.Substring(http_index, (verify_index - http_index) - 1);
		    Console.WriteLine("Parsed License URI: \"{0}\"", temp);
			return temp;
		}
		
		// Extract the metadata URI from TCOP frame data
		// This function is based from CCLicenseLib - Copyright (C) 2004  Steve Griffin
		private static string ParseMetadataURI(string data)
		{
			int verify_index = data.ToLower().IndexOf("verify at ");
			if(verify_index <= 0) {
			    throw new ArgumentException("No metadata was found in Copyright tag at parsing metadata url");
			}
			
			string temp = data.Substring(verify_index + 10, data.Length -
			                           (verify_index + 10));

            Console.WriteLine("Parsed Metadata URI: \"{0}\"", temp);
			return temp;
		}
	}	
}
