Public Class UDPClass

    Dim oSocketIn As System.Net.Sockets.UdpClient
    Dim oSocketOut As System.Net.Sockets.UdpClient
    Dim bStop As Boolean = False
    Public Event UDPDataArrival(ByVal sUDPData As String)

    Dim oReply() As Byte

    Public Sub New()
        oSocketIn = New System.Net.Sockets.UdpClient(5555)
        oSocketOut = New System.Net.Sockets.UdpClient
        'Hostname
        Dim sHostName As String
        sHostName = System.Net.Dns.GetHostName
        'IP Address
        Dim oEntry As System.Net.IPHostEntry = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName)
        Dim sIP As String
        sIP = oEntry.AddressList.GetValue(0).ToString()
        'MAC Address
        Dim sMAC As String
        sMAC = ""
        Dim oSearcher As New System.Management.ManagementObjectSearcher("select *  from Win32_NetworkAdapterConfiguration ")
        Dim oMObject As System.Management.ManagementObject
        For Each oMObject In oSearcher.Get()
            If (CType(oMObject("IPEnabled"), Boolean) = True) Then
                Console.WriteLine("Adapter: " + oMObject.Item("Description").ToString)
                If oMObject.Item("Description").ToString <> "Microsoft Loopback Adapter" Then
                    sMAC = CType(oMObject.Item("MACAddress"), String).Replace(":", "")
                    Console.WriteLine(sMAC)
                    Exit For
                End If
            End If
        Next
        Dim s As String
        s = "<READER><HOSTNAME>" + sHostName + "</HOSTNAME><IP>" + sIP + "</IP><MAC>" + sMAC + "</MAC></READER>"
        ReDim oReply(s.Length())
        Dim c() As Char
        c = s.ToCharArray()
        Dim i As Integer
        For i = 0 To s.Length() - 1
            oReply(i) = CByte(Asc(c(i)))
        Next
    End Sub

    Public Sub UDPListen()
        Do
            Dim oData() As Byte = oSocketIn.Receive(New System.Net.IPEndPoint(System.Net.IPAddress.Any, 5555))
            Console.WriteLine("Data received")
            Dim bData As Byte
            Dim sData As String = ""
            For Each bData In oData
                sData = sData + Chr(CInt(Val(Convert.ToString(bData))))
            Next
            Console.WriteLine(sData)
            RaiseEvent UDPDataArrival(sData)
            If sData.Equals("_QUERY_COGNEX") Then
                Microsoft.VisualBasic.Randomize()
                System.Threading.Thread.Sleep(CInt(100 * Microsoft.VisualBasic.Rnd()))
                oSocketOut.Send(oReply, oReply.Length(), New System.Net.IPEndPoint(System.Net.IPAddress.Parse("255.255.255.255"), 5556))
            End If
        Loop Until bStop
    End Sub

    Public Sub Shutdown()
        oSocketIn.Close()
        oSocketOut.Close()
        'System.Threading.Thread.CurrentThread.Abort()
    End Sub

    Protected Overrides Sub Finalize()
        oSocketIn = Nothing
        oSocketOut = Nothing
        MyBase.Finalize()
    End Sub

End Class

