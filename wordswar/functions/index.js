/* eslint-disable no-constant-condition */
/* eslint-disable linebreak-style */
/* eslint-disable no-undef */
/* eslint-disable linebreak-style */
/* eslint-disable max-len */
// Import the required modules
const functions = require("firebase-functions");
const admin = require("firebase-admin");

// Initialize the Firebase Admin SDK
admin.initializeApp();

// Define the scheduled function
const PAGE_SIZE = 500; // Number of documents per batch

exports.grantDailySpinTicket = functions.pubsub.schedule("every 24 hours").onRun(async (context) => {
  const usersRef = admin.firestore().collection("users");
  let lastDoc = null;

  try {
    while (true) {
      const query = lastDoc ? usersRef.orderBy(admin.firestore.FieldPath.documentId()).startAfter(lastDoc).limit(PAGE_SIZE) : usersRef.limit(PAGE_SIZE);
      const usersSnapshot = await query.get();

      if (usersSnapshot.empty) {
        break; // Exit loop when no more documents
      }

      const batch = admin.firestore().batch();

      usersSnapshot.forEach((doc) => {
        const userRef = doc.ref;
        batch.get(userRef).then((userDoc) => {
          if (!userDoc.exists) {
            console.log(`User document not found: ${userRef.id}`);
            return;
          }

          const userData = userDoc.data();
          if (userData.spinTicket === undefined || userData.spinTicket === 0) {
            batch.update(userRef, {
              spinTicket: admin.firestore.FieldValue.increment(1),
            });
          } else {
            console.log(`User ${userRef.id} already has a spin ticket.`);
          }
        }).catch((error) => {
          console.error(`Error getting user document ${userRef.id}: `, error);
        });
      });

      await batch.commit();
      console.log("Batch committed.");

      // Update lastDoc to the last document in the batch
      lastDoc = usersSnapshot.docs[usersSnapshot.docs.length - 1];
    }

    console.log("Successfully checked and granted spin tickets to all users.");
  } catch (error) {
    console.error("Error granting spin tickets: ", error);
  }
});

