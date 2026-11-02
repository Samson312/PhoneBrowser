package com.phonebrowser.app.viewmodels

import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.getValue
import androidx.compose.runtime.setValue
import androidx.lifecycle.ViewModel
import com.phonebrowser.app.services.permissions.PhotoPermissionChecker
import dagger.hilt.android.lifecycle.HiltViewModel
import timber.log.Timber
import javax.inject.Inject


@HiltViewModel
class PhotoPermissionViewModel @Inject constructor(
    private val checker: PhotoPermissionChecker
) : ViewModel() {
    val permission: String = checker.permission

    var isGranted by mutableStateOf(checker.isGranted())
        private set

    var isDenied by mutableStateOf(false)
        private set

    fun refresh(){
        isGranted = checker.isGranted()
    }

    fun onPermissionResult(granted: Boolean){
        Timber.i("Photo permission result: granted=%s", granted)
        isGranted = granted

        if(!granted) isDenied = true
    }
}