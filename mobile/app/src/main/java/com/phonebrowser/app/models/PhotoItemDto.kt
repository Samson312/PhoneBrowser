package com.phonebrowser.app.models

import kotlinx.serialization.Serializable

@Serializable
data class PhotoItemDto(
    val photoId:String,
    val fileName:String,
    val dateTakenUtc: Long,
    val sizeBytes: Long
)

