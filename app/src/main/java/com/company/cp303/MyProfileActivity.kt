package com.company.cp303

import android.os.Bundle
import android.util.Log
import android.widget.Button
import android.widget.EditText
import androidx.appcompat.app.AppCompatActivity
import com.google.firebase.database.DatabaseReference
import com.google.firebase.database.FirebaseDatabase

class MyProfileActivity : AppCompatActivity() {

    private lateinit var nameEditText: EditText
    private lateinit var emailEditText: EditText
    private lateinit var saveButton: Button
    private lateinit var database: DatabaseReference

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_my_profile)

        nameEditText = findViewById(R.id.nameEditText)
        emailEditText = findViewById(R.id.emailEditText)
        saveButton = findViewById(R.id.saveButton)

        database = FirebaseDatabase.getInstance().reference

        saveButton.setOnClickListener {
            val name = nameEditText.text.toString().trim()
            val email = emailEditText.text.toString().trim()

            if (name.isEmpty() || email.isEmpty()) {
                Log.e(TAG, "Name or email is empty")
                return@setOnClickListener
            }

//            val user = User(name, email)
//            val userId = database.child("users").push().key
//
//            if (userId != null) {
//                database.child("users").child(userId).setValue(user)
//                    .addOnSuccessListener {
//                        Log.d(TAG, "User added to database")
//                        finish()
//                    }
//                    .addOnFailureListener {
//                        Log.e(TAG, "Failed to add user to database", it)
//                    }
//            } else {
//                Log.e(TAG, "Failed to get push key for user")
//            }
        }
    }

    companion object {
        private const val TAG = "MyProfileActivity"
    }
}