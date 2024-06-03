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

const functions = require("firebase-functions");
const admin = require("firebase-admin");
admin.initializeApp();
const database = admin.database();

exports.checkTopicAndWordExistence = functions.https.onCall(async (data, context) => {
  const topicName = data.topicName;
  const word = data.word;
  console.log("topic name: " + topicName + "word : " + word);
  try {
    console.log(`Checking existence of topic '${topicName}' and word '${word}'`);

    const topicSnapshot = await admin.database().ref(`/topics/${topicName}`).once("value");
    if (!topicSnapshot.exists()) {
      console.log(`Topic '${topicName}' does not exist.`);
      throw new Error(`Topic '${topicName}' does not exist.`);
    }

    console.log(`Topic '${topicName}' exists.`);

    const wordSnapshot = await admin.database().ref(`/topics/${topicName}/${word}`).once("value");
    if (!wordSnapshot.exists()) {
      console.log(`Word '${word}' does not exist under the topic '${topicName}'.`);
      throw new Error(`Word '${word}' does not exist under the topic '${topicName}'.`);
    }

    console.log(`Word '${word}' exists under the topic '${topicName}'.`);

    return true; // Both topic and word exist
  } catch (error) {
    console.error(`Error occurred: ${error.message}`);
    throw new functions.https.HttpsError("internal", error.message);
  }
});
