package com.phonebrowser.app.ui.theme

import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.lightColorScheme
import androidx.compose.runtime.Composable
import androidx.compose.ui.graphics.Color

private val AppColorScheme = lightColorScheme(
    primary = Accent,
    onPrimary = Color.White,
    primaryContainer = AccentSubtle,
    onPrimaryContainer = TextPrimary,
    background = Color.White,
    onBackground = TextPrimary,
    surface = Color.White,
    onSurface = TextPrimary,
    onSurfaceVariant = TextSecondary,
    outline = ControlBorder,
    error = ErrorText
)

// Dynamic color is deliberately not used: contrast has to be under our control,
// not derived from the user's wallpaper.
@Composable
fun PhoneBrowserMobileTheme(content: @Composable () -> Unit) {
    MaterialTheme(
        colorScheme = AppColorScheme,
        typography = Typography,
        content = content
    )
}