package com.phonebrowser.app.viewmodels

import androidx.lifecycle.ViewModel

class HomeViewModel: ViewModel() {
    var ssid: String = "Dom-Wifi"


    fun getNetworkName(): String{
        return ssid
    }
}