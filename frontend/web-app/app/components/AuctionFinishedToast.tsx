import { Auction, AuctionFinished } from "@/types";
import Link from "next/link";
import React from "react";
import Image from "next/image";
import { numberWithCommas } from "../lib/numberWithComma";

type Props = {
  finishedAuction: AuctionFinished;
  auction: Auction;
};

export default function AuctionFinishedToast({
  auction,
  finishedAuction,
}: Props) {
  return (
    <Link
      href={`/auctions/details/${auction.id}`}
      className="flex flex-col items-center"
    >
      <div className="flex flex-row items-center gap-2">
        <Image
          src={auction.imageUrl}
          alt="Image of car"
          width={80}
          height={80}
          className="rounded-lg w-auto h-auto"
        />
        <div className="flex flex-col">
          <span>
            New Auction {auction.make} {auction.model} has finishedAuction
            {finishedAuction.itemSold && finishedAuction.amount ? (
              <p>
                Congrats to {finishedAuction.winner} who has won this auction
                for $${numberWithCommas(finishedAuction.amount)}
              </p>
            ) : (
              <p>This item did not sell</p>
            )}
          </span>
        </div>
      </div>
    </Link>
  );
}
