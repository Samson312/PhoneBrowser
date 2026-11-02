package com.phonebrowser.app.viewmodels

import android.content.Context
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.phonebrowser.app.services.foreground.PhoneBrowserForegroundService
import com.phonebrowser.app.storage.SettingsRepository
import dagger.hilt.android.lifecycle.HiltViewModel
import dagger.hilt.android.qualifiers.ApplicationContext
import kotlinx.coroutines.flow.SharingStarted
import kotlinx.coroutines.flow.stateIn
import javax.inject.Inject

@HiltViewModel
class HomeViewModel @Inject constructor(
    @ApplicationContext private val context: Context,
    settingsRepository: SettingsRepository
): ViewModel() {

    val settings = settingsRepository.settingsFlow.stateIn(
        viewModelScope, SharingStarted.WhileSubscribed(5000), null
    )

    init{
        PhoneBrowserForegroundService.start(context)
    }

    fun getNetworkName(): String = "Dom-WiFi"
}