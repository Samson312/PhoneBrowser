package com.phonebrowser.app.ui.nav

sealed class Route(val route: String){
    object Home : Route("home")
}