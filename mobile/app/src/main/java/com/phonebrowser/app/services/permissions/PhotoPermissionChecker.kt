package com.phonebrowser.app.services.permissions

import android.Manifest
import android.content.Context
import android.content.pm.PackageManager
import android.os.Build
import androidx.core.content.ContextCompat
import com.phonebrowser.app.services.media.PhotoRepository
import dagger.hilt.android.qualifiers.ApplicationContext
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext
import javax.inject.Inject
import javax.inject.Singleton

enum class PhotoAccess { FULL, PARTIAL, DENIED }

data class PhotoPermissionState(val access: PhotoAccess, val photoCount: Int)

@Singleton
class PhotoPermissionChecker @Inject constructor(
    @ApplicationContext private val context: Context,
    private val photoRepository: PhotoRepository
) {
    val permissions: Array<String> = when {
        Build.VERSION.SDK_INT >= 34 -> arrayOf(
            Manifest.permission.READ_MEDIA_IMAGES,
            Manifest.permission.READ_MEDIA_VISUAL_USER_SELECTED
        )

        Build.VERSION.SDK_INT >= 33 -> arrayOf(Manifest.permission.READ_MEDIA_IMAGES)

        else -> arrayOf(Manifest.permission.READ_EXTERNAL_STORAGE)
    }
    fun access(): PhotoAccess = when {
        has(permissions[0]) -> PhotoAccess.FULL

        Build.VERSION.SDK_INT >= 34 && has(Manifest.permission.READ_MEDIA_VISUAL_USER_SELECTED) -> PhotoAccess.PARTIAL

        else -> PhotoAccess.DENIED
    }

    suspend fun check(): PhotoPermissionState {
        val access = access()

        val photoCount = if (access != PhotoAccess.DENIED) {
            withContext(Dispatchers.IO) {
                photoRepository.countPhotos()
            }
        } else {
            0
        }

        return PhotoPermissionState(
            access = access,
            photoCount = photoCount
        )
    }

    fun isGranted(): Boolean = access() != PhotoAccess.DENIED
    private fun has(permission: String) =
        ContextCompat.checkSelfPermission(context, permission) == PackageManager.PERMISSION_GRANTED
}