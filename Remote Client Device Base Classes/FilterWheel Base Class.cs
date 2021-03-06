﻿using System;
using System.Collections;
using System.Reflection;
using System.Windows.Forms;

using ASCOM.DeviceInterface;
using RestSharp;

namespace ASCOM.Remote
{
    /// <summary>
    /// ASCOM Remote FilterWheel base class
    /// </summary>
    public class FilterWheelBaseClass : ReferenceCountedObjectBase, IFilterWheelV2
    {
        #region Variables and Constants

        // Constant to set the device type
        private const string DEVICE_TYPE = "FilterWheel";

        // Instance specific variables
        private TraceLoggerPlus TL; // Private variable to hold the trace logger object
        private string DriverNumber; // This driver's number in the series 1, 2, 3...
        private string DriverDisplayName; // Driver description that displays in the ASCOM Chooser.
        private string DriverProgId; // Drivers ProgID
        private string NotConnectedMessage; // Custom message to return if the driver is not connected to the server
        private SetupDialogForm setupForm; // Private variable to hold an instance of the Driver's setup form when invoked by the user
        private RestClient client; // Client to send and receive REST stles messages to / from the remote server
        private int clientNumber; // Unique number for this driver within the locaL server, i.e. across all drivers that the local server is serving
        private bool clientIsConnected;  // Connection state of this driver
        private string URIBase; // URI base unique to this driver

        // Variables to hold values that can be configured by the user through the setup form
        private bool traceState = true;
        private bool debugTraceState = true;
        private string ipAddressString;
        private decimal portNumber;
        private decimal remoteDeviceNumber;
        private string serviceType;
        private int establishConnectionTimeout;
        private int standardServerResponseTimeout;
        private int longServerResponseTimeout;
        private string userName;
        private string password;
        private bool manageConnectLocally;

        #endregion

        #region Initialiser

        /// <summary>
        /// Initializes a new instance of the <see cref="FilterWheelBaseClass"/> class.
        /// Must be public for COM registration.
        /// </summary>
        public FilterWheelBaseClass(string RequiredDriverNumber, string RequiredDriverDisplayName, string RequiredProgId)
        {
            try
            {
                // Initialise variables unique to this particular driver with values passed from the calling class
                DriverNumber = RequiredDriverNumber;
                DriverDisplayName = RequiredDriverDisplayName; // Driver description that displays in the ASCOM Chooser.
                DriverProgId = RequiredProgId;
                NotConnectedMessage = DriverDisplayName + " " + SharedConstants.NOT_CONNECTED_MESSAGE;

                if (TL == null) TL = new TraceLoggerPlus("", string.Format(SharedConstants.TRACELOGGER_NAME_FORMAT_STRING, DriverNumber, DEVICE_TYPE));
                RemoteClientDriver.ReadProfile(clientNumber, TL, DEVICE_TYPE, DriverProgId,
                    ref traceState, ref debugTraceState, ref ipAddressString, ref portNumber, ref remoteDeviceNumber, ref serviceType, ref establishConnectionTimeout, ref standardServerResponseTimeout, ref longServerResponseTimeout, ref userName, ref password, ref manageConnectLocally);

                Version version = Assembly.GetEntryAssembly().GetName().Version;
                TL.LogMessage(clientNumber, DEVICE_TYPE, "Starting initialisation, Version: " + version.ToString());

                clientNumber = RemoteClientDriver.GetUniqueClientNumber();
                TL.LogMessage(clientNumber, DEVICE_TYPE, "This instance's unique client number: " + clientNumber);

                RemoteClientDriver.ConnectToRemoteServer(ref client, ipAddressString, portNumber, serviceType, TL, clientNumber, DEVICE_TYPE, standardServerResponseTimeout, userName, password);

                URIBase = string.Format("{0}{1}/{2}/{3}/", SharedConstants.API_URL_BASE, SharedConstants.API_VERSION_V1, DEVICE_TYPE, remoteDeviceNumber.ToString());
                TL.LogMessage(clientNumber, DEVICE_TYPE, "This devices's base URI: " + URIBase);
                TL.LogMessage(clientNumber, DEVICE_TYPE, "Establish communications timeout: " + establishConnectionTimeout.ToString());
                TL.LogMessage(clientNumber, DEVICE_TYPE, "Standard server response timeout: " + standardServerResponseTimeout.ToString());
                TL.LogMessage(clientNumber, DEVICE_TYPE, "Long server response timeout: " + longServerResponseTimeout.ToString());
                TL.LogMessage(clientNumber, DEVICE_TYPE, "User name: " + userName);
                TL.LogMessage(clientNumber, DEVICE_TYPE, string.Format("Password is Null or Empty: {0}, Password is Null or White Space: {1}", string.IsNullOrEmpty(password), string.IsNullOrWhiteSpace(password)));
                TL.LogMessage(clientNumber, DEVICE_TYPE, string.Format("Password length: {0}", password.Length));

                TL.LogMessage(clientNumber, DEVICE_TYPE, "Completed initialisation");
            }
            catch (Exception ex)
            {
                TL.LogMessageCrLf(clientNumber, DEVICE_TYPE, ex.ToString());
            }
        }

        #endregion

        #region Common properties and methods.

        /// <summary>
        /// 
        /// </summary>
        /// <param name="actionName"></param>
        /// <param name="actionParameters"></param>
        /// <returns></returns>
        public string Action(string actionName, string actionParameters)
        {
            RemoteClientDriver.SetClientTimeout(client, longServerResponseTimeout);
            return RemoteClientDriver.Action(clientNumber, client, URIBase, TL, actionName, actionParameters);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="raw"></param>
        public void CommandBlind(string command, bool raw = false)
        {
            RemoteClientDriver.SetClientTimeout(client, longServerResponseTimeout);
            RemoteClientDriver.CommandBlind(clientNumber, client, URIBase, TL, command, raw);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="raw"></param>
        /// <returns></returns>
        public bool CommandBool(string command, bool raw = false)
        {
            RemoteClientDriver.SetClientTimeout(client, longServerResponseTimeout);
            return RemoteClientDriver.CommandBool(clientNumber, client, URIBase, TL, command, raw);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="raw"></param>
        /// <returns></returns>
        public string CommandString(string command, bool raw = false)
        {
            RemoteClientDriver.SetClientTimeout(client, longServerResponseTimeout);
            return RemoteClientDriver.CommandString(clientNumber, client, URIBase, TL, command, raw);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public bool Connected
        {
            get
            {
                return clientIsConnected;
            }
            set
            {
                clientIsConnected = value;
                if (manageConnectLocally)
                {
                    TL.LogMessage(clientNumber, DEVICE_TYPE, string.Format("The Connected property is being managed locally so the new value '{0}' will not be sent to the remote server", value));
                }
                else // Send the command to the remote server
                {
                    RemoteClientDriver.SetClientTimeout(client, establishConnectionTimeout);
                    if (value) RemoteClientDriver.Connect(clientNumber, client, URIBase, TL);
                    else RemoteClientDriver.Disconnect(clientNumber, client, URIBase, TL);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string Description
        {
            get
            {
                RemoteClientDriver.SetClientTimeout(client, standardServerResponseTimeout);
                string response = string.Format("{0} REMOTE DRIVER: {1}", DriverDisplayName, RemoteClientDriver.Description(clientNumber, client, URIBase, TL));
                TL.LogMessage(clientNumber, "Description", response);
                return response;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string DriverInfo
        {
            get
            {
                RemoteClientDriver.SetClientTimeout(client, standardServerResponseTimeout);
                Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                string remoteString = RemoteClientDriver.DriverInfo(clientNumber, client, URIBase, TL);
                string response = string.Format("{0} Version {1}, REMOTE DRIVER: {2}", DriverDisplayName, version.ToString(), remoteString);
                TL.LogMessage(clientNumber, "DriverInfo", response);
                return response;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string DriverVersion
        {
            get
            {
                RemoteClientDriver.SetClientTimeout(client, standardServerResponseTimeout);
                return RemoteClientDriver.DriverVersion(clientNumber, client, URIBase, TL);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public short InterfaceVersion
        {
            get
            {
                RemoteClientDriver.SetClientTimeout(client, standardServerResponseTimeout);
                return RemoteClientDriver.InterfaceVersion(clientNumber, client, URIBase, TL);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string Name
        {
            get
            {
                string remoteString = RemoteClientDriver.GetValue<string>(clientNumber, client, URIBase, TL, "Name");
                string response = string.Format("{0} REMOTE DRIVER: {1}", DriverDisplayName, remoteString);
                TL.LogMessage(clientNumber, "Name", response);
                return response;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void SetupDialog()
        {
            TL.LogMessage(clientNumber, "SetupDialog", "Connected: " + clientIsConnected.ToString());
            if (clientIsConnected)
            {
                MessageBox.Show("Simulator is connected, setup parameters cannot be changed, please press OK");
            }
            else
            {
                TL.LogMessage(clientNumber, "SetupDialog", "Creating setup form");
                using (setupForm = new SetupDialogForm(TL))
                {
                    // Pass the setup dialogue data into the form
                    setupForm.DriverDisplayName = DriverDisplayName;
                    setupForm.TraceState = traceState;
                    setupForm.DebugTraceState = debugTraceState;
                    setupForm.ServiceType = serviceType;
                    setupForm.IPAddressString = ipAddressString;
                    setupForm.PortNumber = portNumber;
                    setupForm.RemoteDeviceNumber = remoteDeviceNumber;
                    setupForm.EstablishConnectionTimeout = establishConnectionTimeout;
                    setupForm.StandardTimeout = standardServerResponseTimeout;
                    setupForm.LongTimeout = longServerResponseTimeout;
                    setupForm.UserName = userName;
                    setupForm.Password = password;
                    setupForm.ManageConnectLocally = manageConnectLocally;

                    TL.LogMessage(clientNumber, "SetupDialog", "Showing Dialogue");
                    var result = setupForm.ShowDialog();
                    TL.LogMessage(clientNumber, "SetupDialog", "Dialogue closed");
                    if (result == DialogResult.OK)
                    {
                        TL.LogMessage(clientNumber, "SetupDialog", "Dialogue closed with OK status");

                        // Retrieve revised setup data from the form
                        traceState = setupForm.TraceState;
                        debugTraceState = setupForm.DebugTraceState;
                        serviceType = setupForm.ServiceType;
                        ipAddressString = setupForm.IPAddressString;
                        portNumber = setupForm.PortNumber;
                        remoteDeviceNumber = setupForm.RemoteDeviceNumber;
                        establishConnectionTimeout = (int)setupForm.EstablishConnectionTimeout;
                        standardServerResponseTimeout = (int)setupForm.StandardTimeout;
                        longServerResponseTimeout = (int)setupForm.LongTimeout;
                        userName = setupForm.UserName;
                        password = setupForm.Password;
                        manageConnectLocally = setupForm.ManageConnectLocally;

                        // Write the changed values to the Profile
                        TL.LogMessage(clientNumber, "SetupDialog", "Writing new values to profile");
                        RemoteClientDriver.WriteProfile(clientNumber, TL, DEVICE_TYPE, DriverProgId,
                             traceState, debugTraceState, ipAddressString, portNumber, remoteDeviceNumber, serviceType, establishConnectionTimeout, standardServerResponseTimeout, longServerResponseTimeout, userName, password, manageConnectLocally);

                        // Establish new host and device parameters
                        TL.LogMessage(clientNumber, "SetupDialog", "Establishing new host and device parameters");
                        RemoteClientDriver.ConnectToRemoteServer(ref client, ipAddressString, portNumber, serviceType, TL, clientNumber, DEVICE_TYPE, standardServerResponseTimeout, userName, password);
                    }
                    else TL.LogMessage(clientNumber, "SetupDialog", "Dialogue closed with Cancel status");
                }
                if (!(setupForm == null))
                {
                    setupForm.Dispose();
                    setupForm = null;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ArrayList SupportedActions
        {
            get
            {
                RemoteClientDriver.SetClientTimeout(client, standardServerResponseTimeout);
                return RemoteClientDriver.SupportedActions(clientNumber, client, URIBase, TL);
            }
        }
        #endregion

        #region IFilterWheel implementation

        /// <summary>
        /// 
        /// </summary>
        /// <asdasd></asdasd>
        /// <qweqwe></qweqwe>
        public int[] FocusOffsets
        {
            get
            {
                RemoteClientDriver.SetClientTimeout(client, standardServerResponseTimeout);
                return RemoteClientDriver.GetValue<int[]>(clientNumber, client, URIBase, TL, "FocusOffsets");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string[] Names
        {
            get
            {
                RemoteClientDriver.SetClientTimeout(client, standardServerResponseTimeout);
                return RemoteClientDriver.GetValue<string[]>(clientNumber, client, URIBase, TL, "Names");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public short Position
        {
            get
            {
                RemoteClientDriver.SetClientTimeout(client, standardServerResponseTimeout);
                return RemoteClientDriver.GetValue<short>(clientNumber, client, URIBase, TL, "Position");
            }

            set
            {
                RemoteClientDriver.SetClientTimeout(client, standardServerResponseTimeout);
                RemoteClientDriver.SetShort(clientNumber, client, URIBase, TL, "Position", value);
            }
        }

        #endregion
    }
}