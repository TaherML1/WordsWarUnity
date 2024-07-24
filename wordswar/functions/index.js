/* eslint-disable require-jsdoc */
/* eslint-disable max-len */
const {onCall} = require("firebase-functions/v2/https");
const {initializeApp} = require("firebase-admin/app");
const {getFirestore, FieldValue} = require("firebase-admin/firestore");

initializeApp();
const firestore = getFirestore();

// Helper function to generate a 6-character player ID
function generatePlayerId() {
  const characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
  let playerId = "";
  for (let i = 0; i < 6; i++) {
    playerId += characters.charAt(Math.floor(Math.random() * characters.length));
  }
  return playerId;
}

// Function to check if a player ID already exists
async function isPlayerIdUnique(playerId) {
  const snapshot = await firestore.collection("publicProfiles").where("playerId", "==", playerId).get();
  return snapshot.empty;
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

    // Get the user data from the request
    const {username, ...additionalUserData} = data;

    // Set default values for private user data
    const privateUserData = {
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

    // Generate a unique player ID
    let playerId;
    do {
      playerId = generatePlayerId();
    } while (!await isPlayerIdUnique(playerId));

    // Set default values for public user data
    const publicUserData = {
      username: username,
      playerId: playerId,
      // Add any other public fields here
    };

    // Set the private user data in Firestore
    await firestore.collection("users").doc(userId).set(privateUserData);

    // Set the public user data in Firestore
    await firestore.collection("publicProfiles").doc(userId).set(publicUserData);

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
