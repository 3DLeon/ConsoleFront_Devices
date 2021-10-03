using Connector_Devices;
using Entities_Devices;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConsoleFront_Devices
{
	class Program
	{
        const string FUNCTION_ADD = "AddDevices";
        const string FUNCTION_GETALL = "GetDevices";
        const string FUNCTION_GET = "GetDevice";
        const string FUNCTION_DELETE = "DeleteDevices";
        const string FUNCTION_QUIT = "Exit";


        static async Task Main(string[] args)
		{
            string selectedOption;
			string introMessage = "============================================================================================================= \n" +
								  "                                   Welcome to Device Administration App                                       \n" +
								  "=============================================================================================================";
			Console.WriteLine(introMessage);

            do {
                selectedOption = ChooseOption();

                switch (selectedOption) {
                    case FUNCTION_ADD:
                        await DeployAddFunction();
                        break;

                    case FUNCTION_GETALL:
                        await DeployGetAllFunction();
                        break;

                    case FUNCTION_GET:
                        await DeployGetFunction();
                        break;

                    case FUNCTION_DELETE:
                        await DeployDeleteFunction();
                        break;

                    default:
                        Environment.Exit(0);
                        break;
                }
            } while (1 == 1);   
		}

        private static string ChooseOption()
        {
            string selectedOption = null;
            bool isCorrect = true;

            do {
                Console.WriteLine("\n\n   How do you choose to procceed?:\n");
                Console.WriteLine("\t1. Add a new Device");
                Console.WriteLine("\t2. Get the complete list of Devices");
                Console.WriteLine("\t3. Get a specific Device");
                Console.WriteLine("\t4. Delete Devices");
                Console.WriteLine("\t5. Exit App");

                try {
                    string textLine = Console.ReadLine().Trim();
                    isCorrect = true;

                    switch (textLine.ToUpper()) {
                        case "1":
                        case "1.":
                        case "ADD A NEW DEVICE":
                        case "1. ADD A NEW DEVICE":
                        case "ADD":
                            selectedOption = FUNCTION_ADD;
                            break;

                        case "2":
                        case "2.":
                        case "GET THE COMPLETE LIST OF DEVICES":
                        case "2. GET THE COMPLETE LIST OF DEVICES":
                        case "GET ALL":
                            selectedOption = FUNCTION_GETALL;
                            break;

                        case "3":
                        case "3.":
                        case "GET A SPECIFIC DEVICE":
                        case "3. GET A SPECIFIC DEVICE":
                        case "GET":
                            selectedOption = FUNCTION_GET;
                            break;

                        case "4":
                        case "4.":
                        case "DELETE DEVICES":
                        case "4. DELETE DEVICES":
                        case "DELETE":
                            selectedOption = FUNCTION_DELETE;
                            break;

                        case "5":
                        case "5.":
                        case "EXIT APP":
                        case "5. EXIT APP":
                        case "EXIT":
                            selectedOption = FUNCTION_QUIT;
                            break;

                        default:
                            Console.Clear();
                            Console.WriteLine("\n   Invalid option. Try again.");
                            isCorrect = false;
                            break;
                    }
                }
                catch (Exception ex) {
                    Console.Clear();
                    Console.WriteLine("\n   Invalid option. Try again.");
                    isCorrect = false;
                }

            } while (selectedOption == null || !isCorrect);

            Console.Clear();
            return selectedOption;
        }

        private static async Task DeployAddFunction()
        {
            string type = null; string serialNumber = null; string firmwareVersion = null; bool state = false; string ip = null; string port = null;
            bool resume = true;

            Console.WriteLine("\n\n   Introduce a new Serial Number for the Device you wish to create or write 'Cancel' at any moment to cancel the operation:\n");
            serialNumber = WaitForCommand();

            if (serialNumber != "Cancel") {
                Device availabilityCheck = await ConnectorDevices.GetDevice("SerialNumber", serialNumber, new System.Threading.CancellationToken());
                resume = availabilityCheck == null;

                if (!resume) Console.WriteLine("\n\n   The introduced Serial Number already exists.\n");
            } else {
                resume = false;
			}

            if (resume) {
                type = SelectDeviceType();
				if (type == "Cancel") resume = false;
			}

            if (resume) {
                Console.WriteLine("\n\n   Firmware Version of the new Device:\n");
                firmwareVersion = WaitForCommand();
                if (firmwareVersion == "Cancel") resume = false;
            }

            if (resume) {
                string stateText = SelectDeviceState();

				switch (stateText) {
                    case "On": state = true; break;
                    case "Off": state = false; break;
                    case "Cancel": resume = false; break;
                }
            }

            if (type == "Gateways") {
                if (resume) {
                    Console.WriteLine("\n\n   Introduce the Gateways Device IP:\n");
                    ip = WaitForCommand();
                    if (ip == "Cancel") resume = false;
                }

                if (resume) {
                    Console.WriteLine("\n\n   Introduce the Gateways Device Port:\n");
                    port = WaitForCommand();
                    if (port == "Cancel") resume = false;
                }
            }

            if (resume) {
				try {
                    Device newDevice = new Device(type, serialNumber, firmwareVersion, state, ip, port);
                    await ConnectorDevices.AddDevice(newDevice, new System.Threading.CancellationToken());

                    Console.WriteLine("\n\n   The Device was successfully created.\n");
                }
				catch (Exception ex) {
                    Console.WriteLine("\n\n   Error while creating the Device:\n" + ex.Message);
                }
            }
        }
		
		private static async Task DeployGetAllFunction()
        {
            Console.WriteLine("\n   Retrieving data, please wait...");
            List<Device> devicesList = await ConnectorDevices.GetDevices(new System.Threading.CancellationToken());

            if (devicesList != null) {
                Console.WriteLine("\n\n   Devices Total Count: {0}", devicesList.Count.ToString());

                foreach (Device device in devicesList) {
                    string state;
                    if (device.State) state = "ON"; else state = "OFF";

                    Console.WriteLine("\n\n    Device Serial Number: {0} \n\t- Id: {1} \n\t- Type: {2} \n\t- FirmwareVersion: {3} \n\t- State: {4}",
                                        device.SerialNumber, device.Id, device.Type, device.FirmwareVersion, state);
                    if (device.Type == "Gateways") Console.WriteLine("\n\t- IP: {0} \n\t- Port: {1}", device.IP, device.Port);
                }
            } else {
                Console.WriteLine("\n\n   There are no existing Devices.");
            }           
        }

        private static async Task DeployGetFunction()
        {
            Console.WriteLine("\n\n   Introduce the Serial Number of the Device you wish to retrieve or write 'Cancel' to cancel the operation:\n");
            bool isCorrect = true;
            string propertyName = "SerialNumber"; string propertyValue = null;


            do {
                try {
                    string textLine = Console.ReadLine().Trim();
                    isCorrect = true;

                    switch (textLine.ToUpper()) {
                        case "CANCEL":
                            propertyValue = "Cancel";
                            Console.WriteLine("\n   Operation cancelled.");
                            break;

                        case "":
                            Console.WriteLine("\n   Invalid text. Try again.");
                            isCorrect = false;
                            break;

                        default:
                            propertyValue = textLine;
                            Console.WriteLine("\n   Retrieving data, please wait...");
                            break;
                    }
                }
                catch (Exception ex) {
                    Console.WriteLine("\n   Invalid text. Try again.");
                    isCorrect = false;
                }
            } while (propertyValue == null || !isCorrect);

            if(propertyValue != "Cancel") {
                Device device = await ConnectorDevices.GetDevice(propertyName, propertyValue, new System.Threading.CancellationToken());
                if (device != null) {
                    string state;
                    if (device.State) state = "ON"; else state = "OFF";

                    Console.WriteLine("\n\n    Device Serial Number: {0} \n\t- Id: {1} \n\t- Type: {2} \n\t- FirmwareVersion: {3} \n\t- State: {4}",
                                        device.SerialNumber, device.Id, device.Type, device.FirmwareVersion, state);
                    if (device.Type == "Gateways") Console.WriteLine("\n\t- IP: {0} \n\t- Port: {1}", device.IP, device.Port);

                }
                else {
                    Console.WriteLine("\n   The introduced Serial Number was not found.");
                }
            }         
        }

        private static async Task DeployDeleteFunction()
        {
            string propertyName = "SerialNumber"; string propertyValue = null;
            bool resume = true;

            Console.WriteLine("\n\n   Introduce the Serial Number of the Device you wish to delete or write 'Cancel' to cancel the operation:\n");

            propertyValue = WaitForCommand();

            if (propertyValue != "Cancel") {
				try {
                    Device availabilityCheck = await ConnectorDevices.GetDevice("SerialNumber", propertyValue, new System.Threading.CancellationToken());
                    resume = availabilityCheck != null;

                    if (!resume) {
                        Console.WriteLine("\n\n   The introduced Serial Number doesn't exist.\n");
                    } else {
                        await ConnectorDevices.DeleteDevice(propertyName, propertyValue, new System.Threading.CancellationToken());
                        Console.WriteLine("\n\n   The Device was successfully deleted.\n");
                    }             
                }
				catch (Exception ex) {
                    Console.WriteLine("\n\n   Error while creating the Device:\n" + ex.Message);
				}
			} 
            
        }

        private static string SelectDeviceType()
        {
            bool isCorrect = true;
            string text = null;

            Console.WriteLine("\n\n   Write a Type for the new Device (Electric/Water/Gateways):\n");

            do {
                try {
                    string textLine = Console.ReadLine().Trim();
                    isCorrect = true;

                    switch (textLine.ToUpper()) {
                        case "CANCEL":
                            text = "Cancel";
                            Console.WriteLine("\n   Operation cancelled.");
                            break;

                        case "GATEWAYS":
                            text = "Gateways";
                            break;

                        case "ELECTRIC":
                            text = "Electric";
                            break;

                        case "WATER":
                            text = "Water";
                            break;

                        default:
                            Console.WriteLine("\n   Invalid text. Try again.");
                            isCorrect = false;
                            break;
                    }
                }
                catch (Exception ex) {
                    Console.WriteLine("\n   Invalid text. Try again.");
                    isCorrect = false;
                }
            } while (text == null || !isCorrect);

            return text;
        }

        private static string SelectDeviceState()
        {
            bool isCorrect = true;
            string text = null;

            Console.WriteLine("\n\n   Select a State for the new Device (On/Off):\n");

            do {
                try {
                    string textLine = Console.ReadLine().Trim();
                    isCorrect = true;

                    switch (textLine.ToUpper()) {
                        case "CANCEL":
                            text = "Cancel";
                            Console.WriteLine("\n   Operation cancelled.");
                            break;

                        case "ON":
                            text = "On";
                            break;

                        case "OFF":
                            text = "Off";
                            break;

                        default:
                            Console.WriteLine("\n   Invalid text. Try again.");
                            isCorrect = false;
                            break;
                    }
                }
                catch (Exception ex) {
                    Console.WriteLine("\n   Invalid text. Try again.");
                    isCorrect = false;
                }
            } while (text == null || !isCorrect);

            return text;
        }

        private static string WaitForCommand()
        {
            bool isCorrect = true;
            string text = null;

            do {
                try {
                    string textLine = Console.ReadLine().Trim();
                    isCorrect = true;

                    switch (textLine.ToUpper()) {
                        case "CANCEL":
                            text = "Cancel";
                            Console.WriteLine("\n   Operation cancelled.");
                            break;

                        case "":
                            Console.WriteLine("\n   Invalid text. Try again.");
                            isCorrect = false;
                            break;

                        default:
                            text = textLine;
                            break;
                    }
                }
                catch (Exception ex) {
                    Console.WriteLine("\n   Invalid text. Try again.");
                    isCorrect = false;
                }
            } while (text == null || !isCorrect);

            return text;
        }
    }
}
