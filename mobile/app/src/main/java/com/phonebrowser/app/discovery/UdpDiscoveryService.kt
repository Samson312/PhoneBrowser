package com.phonebrowser.app.discovery

import android.os.Build
import kotlinx.coroutines.*
import java.net.DatagramPacket
import java.net.DatagramSocket
import java.net.InetAddress

class UdpDiscoveryService(private val port: Int = 2000) {
    private var job: Job? = null

    fun startBroadcasting(scope: CoroutineScope, onLog: (String) -> Unit){
        job = scope.launch(Dispatchers.IO) {
            val socket = DatagramSocket().apply { broadcast = true }
            val message = "${Build.BRAND}"
            val data = message.toByteArray()
            val address = InetAddress.getByName("255.255.255.255")

            while (isActive) {
                try {
                    socket.send(DatagramPacket(data, data.size, address, port))
                    withContext(Dispatchers.Main) { onLog("Wysłano: $message") }
                } catch (e: Exception) {
                    withContext(Dispatchers.Main) { onLog("Błąd: ${e.message}") }
                }
                delay(2000)
            }
            socket.close()
        }
    }

    fun stop() {
        job?.cancel()
    }
}