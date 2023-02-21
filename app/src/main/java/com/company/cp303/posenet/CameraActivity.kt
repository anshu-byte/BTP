package com.company.cp303.posenet

import android.os.Bundle
import android.view.WindowManager
import androidx.appcompat.app.AppCompatActivity
import com.company.cp303.R

class CameraActivity : AppCompatActivity() {

  override fun onCreate(savedInstanceState: Bundle?) {
    super.onCreate(savedInstanceState)
    setContentView(R.layout.activity_camera)
    window.addFlags(WindowManager.LayoutParams.FLAG_KEEP_SCREEN_ON)
    savedInstanceState ?: supportFragmentManager.beginTransaction()
      .replace(R.id.container, PosenetActivity())
      .commit()
  }
}
