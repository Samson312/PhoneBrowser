package com.phonebrowser.app.ui.nav

sealed class Route(val route: String){
    object PhotoPermission : Route("photo_permission")
    object InitialConfig: Route("initial_config")
    object Home : Route("home")
    object Pairing : Route("pairing")

    object Connected : Route("connected")
}