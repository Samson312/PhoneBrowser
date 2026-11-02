package com.phonebrowser.app.services.permissions

import android.Manifest
import android.content.Context
import android.content.pm.PackageManager
import android.os.Build
import androidx.core.content.ContextCompat
import dagger.hilt.android.qualifiers.ApplicationContext
import javax.inject.Inject
import javax.inject.Singleton

@Singleton
class PhotoPermissionChecker @Inject constructor(
    @ApplicationContext private val context: Context
) {
    val permission: String = if(Build.VERSION.SDK_INT >= 33)
        Manifest.permission.READ_MEDIA_IMAGES
    else
        Manifest.permission.READ_EXTERNAL_STORAGE

    fun isGranted(): Boolean =
        ContextCompat.checkSelfPermission(context, permission) == PackageManager.PERMISSION_GRANTED
}