using System;
using System.Diagnostics;
using System.Net.NetworkInformation;

namespace Scripts
{
    class Scripts {         
        static void Main(string[] args)
        {
            // //Debug
            // Console.WriteLine("Butt");

            //Updates or installs Windows Subsystem for Linux
            Process wslInstall = new Process();
            wslInstall.StartInfo.FileName = "msiexec";
            wslInstall.StartInfo.Arguments = "/i wsl_update_x64.msi";
            wslInstall.Start();
            wslInstall.WaitForExit();
            
            //Sets Windows Subsytem for Linux to version 2
            Process setWslDefault = new Process();
            setWslDefault.StartInfo.FileName = "CMD.exe";
            setWslDefault.StartInfo.Arguments = "/C wsl --set-default-version 2";
            setWslDefault.Start();
            setWslDefault.WaitForExit();

            //Runs Docker Installer
            Process dockInstall = new Process();
            dockInstall.StartInfo.FileName = "DockerDesktopInstaller";
            dockInstall.Start();
            dockInstall.WaitForExit();

            //Runs Docker Desktop
            Process runDock = new Process();
            runDock.StartInfo.FileName = "C:\\Program Files\\Docker\\Docker\\Docker Desktop.exe";
            runDock.Start();

            //Gets pihole through docker
            Process getPihole = new Process();
            getPihole.StartInfo.FileName = "CMD.exe";
            getPihole.StartInfo.Arguments = "/C docker pull pihole/pihole";
            getPihole.Start();
            getPihole.WaitForExit();

            Console.WriteLine();

            //Enumerate network interfaces *StackOverflow
            foreach(NetworkInterface iFace in NetworkInterface.GetAllNetworkInterfaces())
            {
                if(iFace.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || iFace.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                {
                    Console.Write(iFace.Name + ": ");
                    foreach (UnicastIPAddressInformation ip in iFace.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                            {
                                Console.WriteLine(ip.Address.ToString());
                            }
                    }
                }  
            }

            Console.WriteLine();

            //Gets some user input for certain settings
            Console.Write("Enter your interface from list eg. Wi-Fi or Ethernet: ");
            string userIFace = Console.ReadLine();

            Console.Write("Enter your IP address eg. 192.168.254.13: ");
            string userIp = Console.ReadLine();
            string endUserIp = userIp.Split('.').Last();
            int userIpInt = Int32.Parse(endUserIp) + 54;
            userIp = (userIp.Remove(userIp.LastIndexOf(".") + 1));
            userIp = userIp + userIpInt.ToString();
            Console.WriteLine(userIp);

            Console.Write("Enter password for your pihole: ");
            string userPass = Console.ReadLine();

            //Configure network interface ip for pihole
            //The 255.255.255.0 and 192.168.254.254 are hardcoded to my Subnet and Gateway, just change them to whatever your interface is
            //It's late and I'll get those to be automatic later, I'm tired
            Process confInterface = new Process();
            confInterface.StartInfo.FileName = "CMD.exe";
            confInterface.StartInfo.Arguments = "/C netsh interface ipv4 set address name=" + "\"" + userIFace + "\"" + " static " + userIp + " 255.255.255.0 192.168.254.254 store=persistent";
            confInterface.StartInfo.Verb = "runas";
            confInterface.StartInfo.UseShellExecute = true;
            confInterface.Start();
            confInterface.WaitForExit();

            Process dnsInterface = new Process();
            dnsInterface.StartInfo.FileName = "CMD.exe";
            dnsInterface.StartInfo.Arguments = "/C netsh interface ipv4 set dnsservers name=" + "\"" + userIFace + "\"" + " source=static address=" + "\"" + userIp + "\"" + " validate=no";
            dnsInterface.StartInfo.Verb = "runas";
            dnsInterface.StartInfo.UseShellExecute = true;
            dnsInterface.Start();
            dnsInterface.WaitForExit();

            Process dnsInterfaceTwo = new Process();
            dnsInterfaceTwo.StartInfo.FileName = "CMD.exe";
            dnsInterfaceTwo.StartInfo.Arguments = "/C netsh interface ipv4 add dnsservers name=" + "\"" + userIFace + "\"" + " address=\"1.1.1.1\" validate=no index=2";
            dnsInterfaceTwo.StartInfo.Verb = "runas";
            dnsInterfaceTwo.StartInfo.UseShellExecute = true;
            dnsInterfaceTwo.Start();
            dnsInterfaceTwo.WaitForExit();

            //Gets sets Pihole to new configuration
            Process confPihole = new Process();
            confPihole.StartInfo.FileName = "CMD.exe";
            confPihole.StartInfo.Arguments = "/C docker run -d --name pihole -e ServerIP=" + userIp + " -e WEBPASSWORD=" + userPass + " -e DNS1=127.17.0.1 -e DNS2=1.1.1.1 -e DNS3=1.0.0.1 -p 80:80 -p 53:53/tcp -p 53:53/udp -p 443:443 --restart=unless-stopped pihole/pihole:latest";
            confPihole.StartInfo.Verb = "runas";
            confPihole.Start();
            confPihole.WaitForExit();


            ProcessStartInfo openAdmin = new ProcessStartInfo
            {
                FileName = "http://127.17.0.1/admin/",
                UseShellExecute = true
            };
            Process.Start(openAdmin);





        }
    }
}