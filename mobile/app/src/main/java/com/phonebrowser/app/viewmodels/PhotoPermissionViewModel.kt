package com.phonebrowser.app.viewmodels

import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.getValue
import androidx.compose.runtime.setValue
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.phonebrowser.app.services.permissions.PhotoAccess
import com.phonebrowser.app.services.permissions.PhotoPermissionChecker
import com.phonebrowser.app.services.permissions.PhotoPermissionState
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.launch
import kotlinx.io.IOException
import timber.log.Timber
import javax.inject.Inject


@HiltViewModel
class PhotoPermissionViewModel @Inject constructor(
    private val checker: PhotoPermissionChecker
) : ViewModel() {
    val permissions: Array<String> = checker.permissions

    var state by mutableStateOf<PhotoPermissionState?>(null)
        private set

    var isDenied by mutableStateOf(false)
        private set

    private var requested = false

    fun refresh() {
        viewModelScope.launch {
            val newState = checker.check()
            state = newState
            isDenied = requested && newState.access == PhotoAccess.DENIED
        }
    }

    fun onPermissionResult() {
        requested = true
        refresh()
    }
}