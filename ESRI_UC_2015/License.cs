using System;

using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.ADF.COMSupport;
using Miner.Interop;

namespace SE.ArcGISPro
{
    public class License
    {

        //private static readonly log4net.ILog _log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private IAoInitialize _AoInitialize;
        private IMMAppInitialize _MMAppInit;

        /// <summary>
        ///  Attempts to checkout an ArcEditor and ArcFM license and returns true if successful.
        /// </summary>
        public bool GetLicenses()
        {
            ESRI.ArcGIS.RuntimeManager.Bind(ESRI.ArcGIS.ProductCode.Desktop);
            return (GetArcGISLicense(esriLicenseProductCode.esriLicenseProductCodeAdvanced) && GetArcFMLicense(mmLicensedProductCode.mmLPArcFM));
        }

        /// <summary>
        ///  Attempts to checkout a license for the specified ESRI product and returns true if successful.
        /// </summary>
        public bool GetArcGISLicense(esriLicenseProductCode prodCode)
        {
            //Create a new AoInitialize object
            try
            {
                _AoInitialize = new AoInitializeClass();
            }//The initialization object
            catch
            {
                //_log.Warn("Warning: Unable to initialize Arc Objects. License cannot be checked out.");
                return false;
            }

            if (_AoInitialize == null)
            {
                //_log.Warn("Warning: Unable to initialize Arc Objects. License cannot be checked out.");
                return false;
            }
            //Determine if the product is available
            esriLicenseStatus licenseStatus = (esriLicenseStatus)_AoInitialize.IsProductCodeAvailable(prodCode);
            if (licenseStatus == esriLicenseStatus.esriLicenseAvailable)
            {
                licenseStatus = (esriLicenseStatus)_AoInitialize.Initialize(prodCode);
                if (licenseStatus != esriLicenseStatus.esriLicenseCheckedOut)
                {
                    //_log.Warn("Warning: The license checkout for " + prodCode.ToString() + " failed!");
                    return false;
                }
            }
            else
            {
                //_log.Warn("Warning: The ArcGIS product " + prodCode.ToString() + " is unavailable!");
                return false;
            }

            return true;
        }


        /// <summary>
        /// Attempts to checkout a license for the specified Miner & Miner product and returns true if successful.
        /// </summary>
        public bool GetArcFMLicense(mmLicensedProductCode prodCode)
        {
            try
            {
                _MMAppInit = new MMAppInitializeClass();
            }
            catch
            {
                //_log.Warn("Warning: Unable to initialize ArcFM.  No licenses can be checked out.");
                return false;
            }

            if (_MMAppInit == null)
            {
                //_log.Warn("Warning: Unable to initialize ArcFM.  No licenses can be checked out.");
                return false;
            }
            //Determine if the product license is available or is already checked out
            mmLicenseStatus mmlicenseStatus = (mmLicenseStatus)_MMAppInit.IsProductCodeAvailable(prodCode);
            if (mmlicenseStatus == mmLicenseStatus.mmLicenseCheckedOut)
            {
                return true;
            }
            if (mmlicenseStatus == mmLicenseStatus.mmLicenseAvailable)
            {
                mmlicenseStatus = (mmLicenseStatus)_MMAppInit.Initialize(prodCode);
                if (mmlicenseStatus != mmLicenseStatus.mmLicenseCheckedOut)
                {
                    //_log.Warn("Warning: A license cannot be checked out for M&M product " + prodCode.ToString());
                    return false;
                }
                return true;
            }
            else
            {
                //_log.Warn("Warning: No license is available for M&M product " + prodCode.ToString());
                return false;
            }
        }

        /// <summary>
        /// Releases ArcGIS and ArcFM licenses.
        /// </summary>
        public void ReleaseLicenses()
        {
            //Release COM objects and shut down the AoInitilaize object

            AOUninitialize.Shutdown();
            _MMAppInit.Shutdown();
            _AoInitialize.Shutdown();
        }
    }
}
