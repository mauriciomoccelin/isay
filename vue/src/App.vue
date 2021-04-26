<template>
  <v-app>
    <v-app-bar app color="primary" dark>
      <v-spacer v-if="!$vuetify.breakpoint.mobile"></v-spacer>
      <v-toolbar-title>Signal R | Real Time Chat</v-toolbar-title>
      <v-spacer v-if="!$vuetify.breakpoint.mobile"></v-spacer>
    </v-app-bar>

    <v-main>
      <v-container>
        <v-form @submit.prevent="send">
          <v-row>
            <v-col cols="12" xl="4" lg="4">
              <v-text-field
                outlined
                name="input-7-4"
                label="Message"
                v-model="model.text"
              ></v-text-field>
              <v-btn
                :disabled="!model.text"
                color="success"
                class="mr-4"
                @click="send"
              >
                Send
              </v-btn>
            </v-col>
            <v-col cols="12" xl="4" lg="4">
              <v-list subheader>
                <v-subheader class="headline">Messages</v-subheader>
                <v-list-item v-for="(message, index) in messages" :key="index">
                  <v-list-item-content>
                    <v-list-item-title>
                      {{ message.user }}
                    </v-list-item-title>
                    <v-list-item-subtitle class="text-wrap text-justify">
                      {{ message.text }}
                    </v-list-item-subtitle>
                  </v-list-item-content>
                </v-list-item>
              </v-list>
            </v-col>
          </v-row>
        </v-form>
      </v-container>
    </v-main>

    <v-dialog v-model="dialog" persistent max-width="600px">
      <v-card>
        <v-card-title>
          <span class="headline">User</span>
        </v-card-title>
        <v-card-text>
          <v-row>
            <v-col cols="12">
              <v-text-field
                class="mt-8"
                v-model="model.user"
                label="Your full name"
              />
            </v-col>
          </v-row>
        </v-card-text>
        <v-card-actions>
          <v-spacer></v-spacer>
          <v-btn
            text
            color="success"
            @click="dialog = false"
            :disabled="!hasUserName"
            >Ok</v-btn
          >
        </v-card-actions>
      </v-card>
    </v-dialog>
  </v-app>
</template>

<script lang="ts">
import Vue from "vue";
import * as signalR from "@microsoft/signalr";
import { Component } from "vue-property-decorator";

const connection = new signalR.HubConnectionBuilder()
  .withUrl("https://localhost:5001/chathub")
  .build();

interface Message {
  user: string;
  text: string | null;
  sendOn: Date;
}

@Component
export default class App extends Vue {
  name = "App";

  private dialog: boolean | null = true;
  private messages: Array<Message> = [];
  private model: Message = {
    text: null,
    user: "Anonymous",
    sendOn: new Date(),
  };

  get hasUserName(): boolean {
    const name = this.model.user;
    if (name === "") return false;
    if (name === "Anonymous") return false;
    if (name === null) return false;
    return true;
  }

  received(args: string): void {
    const message: Message = JSON.parse(args);
    this.messages.push(message);
    this.messages = this.messages.sort((a, b) => {
      if (a.sendOn < b.sendOn) return 1;
      if (a.sendOn > b.sendOn) return -1;
      return 0;
    });
  }

  send(): void {
    this.model.sendOn = new Date();
    const args = JSON.stringify(this.model);

    connection.invoke("SendMessage", args);

    this.model.text = "";
    this.model.sendOn = new Date();
  }

  async mounted(): Promise<void> {
    await connection.start();
    connection.on("ReceiveMessage", this.received);
  }
}
</script>
