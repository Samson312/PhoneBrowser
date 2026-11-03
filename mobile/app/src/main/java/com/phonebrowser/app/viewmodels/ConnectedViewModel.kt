package com.phonebrowser.app.viewmodels

import androidx.lifecycle.ViewModel
import com.phonebrowser.app.services.session.ActiveSessionService
import dagger.hilt.android.lifecycle.HiltViewModel
import javax.inject.Inject

@HiltViewModel
class ConnectedViewModel @Inject constructor(
    private val activeSessionService: ActiveSessionService
) : ViewModel() {
    val connectedPeer = activeSessionService.connectedPeer

}