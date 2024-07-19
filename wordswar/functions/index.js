const functions = require("firebase-functions");
const admin = require("firebase-admin");

admin.initializeApp();
const firestore = admin.firestore();

exports.addSpinTicket = functions.https.onCall(async (data, context) => {
  try {
    // Check if the request is authenticated
    if (!context.auth) {
      throw new functions.https.HttpsError(
          "unauthenticated",
          "You must be authenticated to call this function.",
      );
    }

    // Get the user ID from the authenticated user
    const userId = context.auth.uid;

    // Reference to the user's document in Firestore
    const userDocRef = firestore.collection("users").doc(userId);

    // Increment the spin ticket count by 1
    await userDocRef.update({
      spinTicket: admin.firestore.FieldValue.increment(1),
    });

    return {success: true, message: "Spin ticket added successfully."};
  } catch (error) {
    console.error("Error adding spin ticket:", error);
    throw new functions.https.HttpsError(
        "unknown",
        "An error occurred while adding the spin ticket.",
    );
  }
});
