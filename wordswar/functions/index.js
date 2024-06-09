/* eslint-disable no-unused-vars */
/* eslint-disable max-len */
/**
 * Import function triggers from their respective submodules:
 *
 * const {onCall} = require("firebase-functions/v2/https");
 * const {onDocumentWritten} = require("firebase-functions/v2/firestore");
 *
 * See a full list of supported triggers at https://firebase.google.com/docs/functions
 */


// Create and deploy your first functions
// https://firebase.google.com/docs/functions/get-started

// index.js (assuming this is your Cloud Function file name)

/* eslint-disable no-unused-vars */
/* eslint-disable max-len */
/**
 * Import function triggers from their respective submodules:
 *
 * const {onCall} = require("firebase-functions/v2/https");
 * const {onDocumentWritten} = require("firebase-functions/v2/firestore");
 *
 * See a full list of supported triggers at https://firebase.google.com/docs/functions
 */


// Create and deploy your first functions
// https://firebase.google.com/docs/functions/get-started

// index.js (assuming this is your Cloud Function file name)

const {onCall} = require("firebase-functions/v2/https");
const {initializeApp} = require("firebase-admin/app");
const {getFirestore} = require("firebase-admin/firestore");

initializeApp();
const firestore = getFirestore();

exports.setUser2 = onCall(async (request) => {
  const {data, auth} = request;

  try {
    // Check if the request is authenticated
    if (!auth) {
      throw new Error("You must be authenticated to call this function.");
    }

    // Get the user ID from the authenticated user
    const userId = auth.uid;

    // Get the user data from the request
    const {username, ...additionalUserData} = data;

    // Set default values for user data
    const userData = {
      username: username,
      gems: 0,
      coins: 0,
      level: 1,
      scores: 0,
      xp: 0,
      email: auth.token.email || null,
      profileComplete: true,
      ...additionalUserData,
    };

    // Set the user data in Firestore
    await firestore.collection("users").doc(userId).set(userData);

    // Add hints document for the user
    await firestore.collection("users").doc(userId).collection("hints").doc("hintsData").set({
      joker: 0, // Default value for joker
      extraTime: 0, // Default value for extraTime
      // Add more hint fields as needed
    });

    return {success: true, message: "User data and hints set successfully."};
  } catch (error) {
    // Handle errors
    throw new Error("An error occurred while setting user data and hints.");
  }
});


