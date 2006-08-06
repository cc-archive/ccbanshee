/***************************************************************************
 *  Licenses.cs
 *
 *  Copyright (C) 2006 Luke Hoersten
 *  Written by Luke Hoersten <luke.hoersten@gmail.com>
 ****************************************************************************/

/*  THIS FILE IS LICENSED UNDER THE MIT LICENSE AS OUTLINED IMMEDIATELY BELOW: 
 *
 *  Permission is hereby granted, free of charge, to any person obtaining a
 *  copy of this software and associated documentation files (the "Software"),  
 *  to deal in the Software without restriction, including without limitation  
 *  the rights to use, copy, modify, merge, publish, distribute, sublicense,  
 *  and/or sell copies of the Software, and to permit persons to whom the  
 *  Software is furnished to do so, subject to the following conditions:
 *
 *  The above copyright notice and this permission notice shall be included in 
 *  all copies or substantial portions of the Software.
 *
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 *  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 *  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
 *  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 *  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 *  FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 *  DEALINGS IN THE SOFTWARE.
 */
 
using System;
using System.Collections.Generic;

namespace Banshee.Base
{
	public static class Licenses
	{
	    public enum Attributes {
            none = 0,
            nd,
            nd_nc,
            nc,
            nc_sa,
            sa,
            by,
            by_nd,
            by_nc,
            by_nc_nd,
            by_nc_sa,
            by_sa,
            sampling,
            samplingplus,
            nc_samplingplus,
            devnations,
            publicdomain
	    }
	    
	    private static string HTTP_STRING = "http://";
	    private static string LICENSES_STRING = "licenses/";
        private static int    LICENSES_LENGTH = 9;
	    private static string VERIFY_STRING = "verify at ";
        private static int    VERIFY_LENGTH = 10;
	    
        public static void FillTags(TrackInfo track)
        {
            if(track.Copyright == null) {
                    return;
            } else {
                if(track.LicenseUri == null && track.MetadataUri == null) {
                    try {
                        track.LicenseUri = ParseLicenseUri(track.Copyright);
                        track.MetadataUri = ParseMetadataUri(track.Copyright);
                    } catch(LicenseParseException) {
                        return;
                    }
                }
            }
            
            track.License = (Attributes)Enum.Parse(typeof(Attributes),
                                ParseAttributes(track.LicenseUri).Replace("-", "_").Replace("+", "plus"));
        }
        
        private static string ParseAttributes(string data)
        {
            int licenses_index = data.ToLower().IndexOf(LICENSES_STRING);
            if(licenses_index <= 0) {
                throw new LicenseParseException("No attributes were found in Copyright tag.");
            }
            
            int attribute_index = licenses_index + LICENSES_LENGTH;
            return data.Substring(attribute_index, data.IndexOf('/', attribute_index) - attribute_index);
        }

        private static string ParseLicenseUri(string data)
        {
            int verify_index = (data.ToLower()).IndexOf(VERIFY_STRING);
            if(verify_index <= 0) {
                throw new LicenseParseException("No metadata was found in Copyright tag while parsing license URL.");    
            }
            
            int http_index = (data.ToLower()).LastIndexOf(HTTP_STRING, verify_index);
            if(http_index <= 0) {
                throw new LicenseParseException("No license was found in Copyright tag.");
            }
            
            /* The -1 is because LastIndexOf adds 1 for arg test */
            return data.Substring(http_index, (verify_index - http_index) - 1);
        }

        private static string ParseMetadataUri(string data)
        {
            int verify_index = data.ToLower().IndexOf(VERIFY_STRING);
            if(verify_index <= 0) {
                throw new LicenseParseException("No metadata was found in Copyright tag at parsing metadata URL.");
            }
            
            int metadata_index = verify_index + VERIFY_LENGTH;
            return data.Substring(metadata_index, data.Length - (metadata_index));
        }
	}
	                
    public class LicenseParseException : ApplicationException
    {
        public LicenseParseException(string m) : base(m)
        {
        }
    }
}
