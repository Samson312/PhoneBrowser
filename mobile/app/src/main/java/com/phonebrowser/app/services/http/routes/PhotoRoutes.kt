package com.phonebrowser.app.services.http.routes

import android.graphics.Bitmap
import com.phonebrowser.app.services.media.PhotoRepository
import io.ktor.http.ContentType
import io.ktor.server.routing.Route
import io.ktor.http.HttpStatusCode
import io.ktor.server.response.respond
import io.ktor.server.response.respondBytes
import io.ktor.server.response.respondOutputStream
import io.ktor.server.routing.get
import timber.log.Timber

fun Route.photoEndpoints(photoRepository: PhotoRepository) {
    get("/photos") {
        val since = call.request.queryParameters["since"]?.toLongOrNull()
        val photos = photoRepository.listPhotos(since)
        Timber.d("Served /photos — %d items (since=%s)", photos.size, since)
        call.respond(photos)
    }

    get("/photos/{id}/thumbnail") {
        val id = call.parameters["id"]?.toLongOrNull()
            ?: return@get call.respond(HttpStatusCode.BadRequest)

        val bitmap = photoRepository.loadThumbnail(id) ?: run {
            Timber.d("Thumbnail not found for photo %d", id)
            return@get call.respond(HttpStatusCode.NotFound)
        }

        val bytes = java.io.ByteArrayOutputStream().also {
            bitmap.compress(Bitmap.CompressFormat.JPEG, 80, it)
        }.toByteArray()

        call.respondBytes(bytes, ContentType.Image.JPEG)
    }

    get("/photos/{id}/full") {
        val id = call.parameters["id"]?.toLongOrNull()
            ?: return@get call.respond(HttpStatusCode.BadRequest)

        val input = photoRepository.openFullPhotoStream(id) ?: run {
            Timber.w("Full photo stream not found for photo %d", id)
            return@get call.respond(HttpStatusCode.NotFound)
        }

        call.respondOutputStream(ContentType.Image.JPEG) {
            input.use { it.copyTo(this) }
        }
    }
}