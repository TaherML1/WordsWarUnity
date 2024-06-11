// index.js

const {onCall} = require("firebase-functions/v2/https");
const {initializeApp} = require("firebase-admin/app");
const {getFirestore, FieldValue} = require("firebase-admin/firestore");

initializeApp();
const firestore = getFirestore();

exports.processSpinResult = onCall(async (request) => {
  const {data, auth} = request;

  try {
    // Check if the request is authenticated
    if (!auth) {
      throw new Error("You must be authenticated to call this function.");
    }

    // Get the user ID from the authenticated user
    const userId = auth.uid;

    // Get the spin result from the request data
    const {reward} = data;

    // Define the document references
    const userDocRef = firestore.collection("users").doc(userId);
    const hintsDocRef = userDocRef.collection("hints").doc("hintsData");

    // Initialize an update object
    let updateData = {};

    // Determine the update based on the reward
    switch (reward) {
      case "100xp":
        updateData = {xp: FieldValue.increment(100)};
        break;
      case "10 coins":
        updateData = {coins: FieldValue.increment(10)};
        break;
      case "10 gems":
        updateData = {gems: FieldValue.increment(10)};
        break;
      case "bad luck":
        // No reward
        return {success: true, message: "Better luck next time!"};
      case "100 coins":
        updateData = {coins: FieldValue.increment(100)};
        break;
      case "extra hint":
        await hintsDocRef.update({extraTime: FieldValue.increment(1)});
        return {success: true, message: "You won an extra hint!"};
      case "100 gems":
        updateData = {gems: FieldValue.increment(100)};
        break;
      case "joker":
        await hintsDocRef.update({joker: FieldValue.increment(1)});
        return {success: true, message: "You won a Joker!"};
      default:
        throw new Error("Invalid reward type");
    }

    // Update the user's document with the reward
    await userDocRef.update(updateData);

    return {success: true, message: `You won ${reward}!`};
  } catch (error) {
    // Handle errors
    console.error("Error processing spin result:", error);
    throw new Error("An error occurred while processing the spin result.");
  }
});
