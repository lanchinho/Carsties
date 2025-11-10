"use server";

import { auth } from "@/auth";
import { Auction, PagedResult } from "@/types";


export async function getData(query: string ): Promise<PagedResult<Auction>>{
    const res = await fetch(`http://localhost:6001/search?${query}`);
    if(!res.ok){
        console.log("Failed to fetch data");
        return {
            results: [],
            totalCount: 0,            
            pageCount: 0
      };

    } 

    return res.json();
}

export async function updateAuctionTest() : Promise<{status: number, message: string}>{
    const data ={
        milage: Math.floor(Math.random() * 10000) + 1
    }

    const session = await auth();
    const res = await fetch(`http://localhost:6001/auctions/afbee524-5972-4075-8800-7d1f9d7b0a0c`, {
        method: "PUT",
        headers:{
            "Content-Type": "application/json",
            "Authorization": `Bearer ${session?.accessToken}`
        },
        body: JSON.stringify(data)
    });

    if(!res.ok) return {status: res.status, message: res.statusText};

    return {status: res.status, message: res.statusText};
}