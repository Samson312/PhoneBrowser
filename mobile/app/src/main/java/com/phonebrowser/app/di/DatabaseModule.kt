package com.phonebrowser.app.di

import android.content.Context
import androidx.room.Room
import com.phonebrowser.app.storage.PhoneBrowserDatabase
import com.phonebrowser.app.storage.TrustedDeviceDao
import dagger.Module
import dagger.Provides
import dagger.hilt.InstallIn
import dagger.hilt.android.qualifiers.ApplicationContext
import dagger.hilt.components.SingletonComponent
import javax.inject.Singleton

@Module
@InstallIn(SingletonComponent::class)
object DatabaseModule {

    @Provides
    @Singleton
    fun provideDataase(@ApplicationContext context: Context): PhoneBrowserDatabase =
        Room.databaseBuilder(
            context,
            PhoneBrowserDatabase::class.java,
            "phonebrowser.db"
        ).build()

    @Provides
    fun provideTrustedDeviceDao(database: PhoneBrowserDatabase): TrustedDeviceDao =
        database.trustedDeviceDao()
}