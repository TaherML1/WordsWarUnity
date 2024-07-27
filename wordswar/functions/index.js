/* eslint-disable require-jsdoc */
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

// index.js

const {onCall} = require("firebase-functions/v2/https");
const {initializeApp} = require("firebase-admin/app");
const {getFirestore, FieldValue} = require("firebase-admin/firestore");

initializeApp();
const firestore = getFirestore();

// Function to generate a random 6-character player ID
function generatePlayerId() {
  const chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
  let playerId = "";
  for (let i = 0; i < 6; i++) {
    playerId += chars.charAt(Math.floor(Math.random() * chars.length));
  }
  return playerId;
}

// Function to check if a player ID already exists in Firestore
async function isPlayerIdUnique(playerId) {
  const usersRef = firestore.collection("users");
  const querySnapshot = await usersRef.where("playerId", "==", playerId).get();
  return querySnapshot.empty;
}

exports.setUser2 = onCall(async (request) => {
  const {data, auth} = request;

  try {
    // Check if the request is authenticated
    if (!auth) {
      throw new Error("You must be authenticated to call this function.");
    }

    // Get the user ID from the authenticated user
    const userId = auth.uid;

    // Generate a unique player ID
    let playerId = generatePlayerId();
    while (!(await isPlayerIdUnique(playerId))) {
      playerId = generatePlayerId();
    }

    // Get the user data from the request
    const {username, ...additionalUserData} = data;

    // Set default values for user data including tickets and lastRefresh
    const userData = {
      username: username,
      playerId: playerId,
      gems: 0,
      coins: 0,
      level: 1,
      scores: 0,
      xp: 0,
      matchesWon: 0,
      matchesLost: 0,
      spinTicket: 0,
      email: auth.token.email || null,
      lastRefresh: FieldValue.serverTimestamp(), // Set the current server timestamp
      profileComplete: true,
      ...additionalUserData,
    };

    // Set the user data in Firestore
    await firestore.collection("users").doc(userId).set(userData);

    // Add hints document for the user
    await firestore.collection("users").doc(userId).collection("hints").doc("hintsData").set({
      joker: 0, // Default value for joker
      extraTime: 0, // Default value for extraTime
      tickets: 3,
      // Add more hint fields as needed
    });

    return {success: true, message: "User data and hints set successfully."};
  } catch (error) {
    // Handle errors
    throw new Error("An error occurred while setting user data and hints: " + error.message);
  }
});
