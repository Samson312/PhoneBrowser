package com.phonebrowser.app.services.media

import android.content.ContentUris
import android.content.Context
import android.graphics.Bitmap
import android.os.Build
import android.provider.MediaStore
import android.util.Size
import com.phonebrowser.app.models.PhotoItemDto
import dagger.hilt.android.qualifiers.ApplicationContext
import javax.inject.Inject


class PhotoRepository @Inject constructor(
    @ApplicationContext private val context: Context
) {
    fun listPhotos(sinceEpochMillis: Long? = null): List<PhotoItemDto> {
        val projection = arrayOf(
            MediaStore.Images.Media._ID,
            MediaStore.Images.Media.DISPLAY_NAME,
            MediaStore.Images.Media.DATE_TAKEN,
            MediaStore.Images.Media.DATE_ADDED,
            MediaStore.Images.Media.SIZE,
        )

        val effectiveDateExpr =
            "CASE WHEN ${MediaStore.Images.Media.DATE_TAKEN} > 0 " +
            "THEN ${MediaStore.Images.Media.DATE_TAKEN} " +
            "ELSE ${MediaStore.Images.Media.DATE_ADDED} * 1000 END"

        val selection = sinceEpochMillis?.let { "$effectiveDateExpr >= ?" }
        val selectionArgs = sinceEpochMillis?.let { arrayOf(it.toString()) }
        val sortOrder = "$effectiveDateExpr DESC"

        val result = mutableListOf<PhotoItemDto>()

        context.contentResolver.query(
            MediaStore.Images.Media.EXTERNAL_CONTENT_URI,
            projection, selection, selectionArgs, sortOrder
        )?.use { cursor ->
            val idCol = cursor.getColumnIndexOrThrow(MediaStore.Images.Media._ID)
            val nameCol = cursor.getColumnIndexOrThrow(MediaStore.Images.Media.DISPLAY_NAME)
            val dateTakenCol = cursor.getColumnIndexOrThrow(MediaStore.Images.Media.DATE_TAKEN)
            val dateAddedCol = cursor.getColumnIndexOrThrow(MediaStore.Images.Media.DATE_ADDED)
            val sizeCol = cursor.getColumnIndexOrThrow(MediaStore.Images.Media.SIZE)

            while (cursor.moveToNext()) {
                val id = cursor.getLong(idCol)
                val dateTaken = cursor.getLong(dateTakenCol)
                val dateAddedSeconds = cursor.getLong(dateAddedCol)

                val effectiveDate = if (dateTaken > 0) dateTaken else dateAddedSeconds * 1000

                result += PhotoItemDto(
                    photoId = id.toString(),
                    fileName = cursor.getString(nameCol) ?: "zdjecie_$id.jpg",
                    dateTakenUtc = effectiveDate,
                    sizeBytes = cursor.getLong(sizeCol),
                )
            }
        }
        return result
    }

    fun openFullPhotoStream(photoId: Long) =
        context.contentResolver.openInputStream(
            ContentUris.withAppendedId(MediaStore.Images.Media.EXTERNAL_CONTENT_URI, photoId)
        )

    fun loadThumbnail(photoId: Long): Bitmap? {
        val uri = ContentUris.withAppendedId(MediaStore.Images.Media.EXTERNAL_CONTENT_URI, photoId)
        return if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.Q) {
            context.contentResolver.loadThumbnail(uri, Size(256, 256), null)
        } else {
            @Suppress("DEPRECATION")
            MediaStore.Images.Thumbnails.getThumbnail(
                context.contentResolver, photoId, MediaStore.Images.Thumbnails.MINI_KIND, null
            )
        }
    }
}