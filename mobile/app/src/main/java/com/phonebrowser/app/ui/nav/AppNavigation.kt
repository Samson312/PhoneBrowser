package com.phonebrowser.app.ui.nav

import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.navigation.NavHostController
import androidx.navigation.compose.rememberNavController
import androidx.navigation.compose.NavHost
import androidx.navigation.compose.composable
import com.phonebrowser.app.ui.home.MainScreen

@Composable
fun AppNavigation(
    modifier: Modifier = Modifier,
    navController: NavHostController = rememberNavController(),
)
{
    NavHost(
        navController = navController,
        startDestination = Route.Home.route,
        modifier = modifier,
    ){
        composable(Route.Home.route)
        {
            MainScreen()
        }
    }
}