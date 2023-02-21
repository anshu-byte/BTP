package com.company.cp303.posenet

import android.Manifest
import android.app.AlertDialog
import android.app.Dialog
import android.os.Bundle
import androidx.fragment.app.DialogFragment

class ConfirmationDialog : DialogFragment() {

  override fun onCreateDialog(savedInstanceState: Bundle?): Dialog =
    AlertDialog.Builder(activity)
      .setMessage("This app needs camera permission.")
      .setPositiveButton("OK") { _, _ ->
        requireParentFragment().requestPermissions(
          arrayOf(Manifest.permission.CAMERA),
          REQUEST_CAMERA_PERMISSION
        )
      }
      .setNegativeButton("Cancel") { _, _ ->
        requireParentFragment().activity?.finish()
      }
      .create()
}
