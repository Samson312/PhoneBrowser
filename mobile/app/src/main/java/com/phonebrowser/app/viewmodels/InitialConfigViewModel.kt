package com.phonebrowser.app.viewmodels

import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.setValue
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.phonebrowser.app.storage.SettingsRepository
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.SharingStarted
import kotlinx.coroutines.flow.first
import kotlinx.coroutines.flow.stateIn
import kotlinx.coroutines.launch
import javax.inject.Inject

@HiltViewModel
class InitialConfigViewModel @Inject constructor(
    private val settingsRepository: SettingsRepository
): ViewModel() {
    var deviceName by mutableStateOf("")
        private set

    var errorMessage by mutableStateOf<String?>(null)
        private set

    init {
        viewModelScope.launch {
            deviceName = settingsRepository.settingsFlow.first().deviceName
        }
    }

    fun onDeviceNameChange(value: String) {
        deviceName = value
        errorMessage = null
    }

    fun save(onSaved: () -> Unit) {
        if (deviceName.isBlank()) {
            errorMessage = "Podaj nazwę telefonu"
            return
        }
        viewModelScope.launch {
            settingsRepository.saveDeviceName(deviceName.trim())
            onSaved()
        }
    }
}