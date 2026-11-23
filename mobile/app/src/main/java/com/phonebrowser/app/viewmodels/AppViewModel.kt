package com.phonebrowser.app.viewmodels

import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.setValue
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.phonebrowser.app.services.pairing.PairingManager
import com.phonebrowser.app.services.permissions.PhotoAccess
import com.phonebrowser.app.services.permissions.PhotoPermissionChecker
import com.phonebrowser.app.services.session.ActiveSessionService
import com.phonebrowser.app.storage.SettingsRepository
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.flow.SharingStarted
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.first
import kotlinx.coroutines.flow.map
import kotlinx.coroutines.flow.stateIn
import kotlinx.coroutines.launch
import javax.inject.Inject

@HiltViewModel
class AppViewModel @Inject constructor(
    settingsRepository: SettingsRepository,
    pairingManager: PairingManager,
    photoPermissionChecker: PhotoPermissionChecker,
    activeSessionService: ActiveSessionService
): ViewModel() {
    val hasProfile: StateFlow<Boolean?> = settingsRepository.hasProfileFlow.stateIn(
        viewModelScope, SharingStarted.WhileSubscribed(5000), null
    )

    val needsPhotoPermissionScreen: Boolean =
        photoPermissionChecker.access() != PhotoAccess.FULL

    val isConnected: StateFlow<Boolean> = activeSessionService.connectedPeer
        .map{it != null}
        .stateIn(viewModelScope, SharingStarted.WhileSubscribed(5000), activeSessionService.isConnected)


    val incomingRequest = pairingManager.incomingRequest
}